using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace lockdown
{
    public class EncryptionHelper
    {
        private RijndaelManaged rijndael;
        private FileStream output;
        private byte[] buffer;

        private EntityDetails saveEntity;
        private EntityDetails readEntity;
        public EncryptionHelper(EntityDetails readEntity, EntityDetails saveEntity)
        {
            this.saveEntity = saveEntity;
            this.readEntity = readEntity;
        }

        private bool IsDirectory(string path)
        {
            var attrb = File.GetAttributes(path);
            return (attrb & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private void _GetIndices(string path, List<IndexedItem> indices, bool isDirectory, long parentID, ref long thisID)
        {
            if (!isDirectory)
            {
                indices.Add(new IndexedItem(thisID++, parentID, false, path, (new FileInfo(path)).Length));
            }
            else
            {
                indices.Add(new IndexedItem(thisID, parentID, true, path, 0));

                long temp = thisID;
                ++thisID;

                var directories = Directory.GetDirectories(path);
                foreach (var s in directories) _GetIndices(s, indices, true, temp, ref thisID);

                var files = Directory.GetFiles(path);
                foreach (var s in files) _GetIndices(s, indices, false, temp, ref thisID);
            }
        }

        public List<IndexedItem> GetIndices(string path)
        {
            if (!IsDirectory(path)) 
                return new List<IndexedItem>() { new IndexedItem(0, -1, false, path, (new FileInfo(path)).Length) };

            var indices = new List<IndexedItem>();
            long ID = 0;
            _GetIndices(path, indices, true, -1, ref ID);
            return indices;
        }

        public void GenerateFileIndices(List<IndexedItem> indices)
        {
            long currentIndex = 0;
            foreach (var item in indices)
            {
                if (item.isDirectory) continue;

                item.sectorStart = currentIndex;
                int delta = (rijndael.BlockSize / 8);
                long sectorSize = (item.size / delta) + 1;
                currentIndex += sectorSize;
            }
        }

        public void CreateAESInstance(string password)
        {
            const int bufferLength = 32 * 1024 * 1024;
            buffer = new byte[bufferLength];

            // Initialize Crypto
            rijndael = new RijndaelManaged();

            // Generate salt
            byte[] salt = new byte[32];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }


            // Generate key
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(password, salt, 1000);

            rijndael.Key = k1.GetBytes(32);
            rijndael.GenerateIV();

            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;

            Debug.WriteLine(rijndael.BlockSize);

            
            var fileName = readEntity.shortName.Split('.');
            string name = "";
            if (!readEntity.isDirectory)
            {
                for (int i = 0; i < fileName.Length - 1; ++i)
                    name += fileName[i];
            }
            else
            {
                name = readEntity.shortName;
            }

            output = new FileStream($"{saveEntity.entityName}\\{name}.lock", FileMode.Create);

            var hasher = SHA256.Create();
            // Generate the hash
            var suffixedPassword = new byte[32 + password.Length];
            var passwordBytes = Encoding.UTF8.GetBytes(password);   // Password + Salt (Salt Suffix)
            for (int i = 0; i < passwordBytes.Length; ++i) suffixedPassword[i] = passwordBytes[i];
            for (int i = 0; i < 32; ++i) suffixedPassword[passwordBytes.Length + i] = salt[i];
            var hash = hasher.ComputeHash(suffixedPassword);

            output.Write(hash);               // 32 bytes
            output.Write(rijndael.IV);        // 16 bytes
            output.Write(salt);               // 32 bytes

            hasher.Dispose();
            output.Flush();
        }

        public async Task WriteIndices(string path, List<IndexedItem> indices)
        {
            string index_string = "";
            foreach (IndexedItem i in indices)
            {
                index_string += i.Serialize();
            }
            index_string = $"{index_string.Length.ToString().PadLeft(15, '0')}{index_string}";

            using (CryptoStream cs = new CryptoStream(output, rijndael.CreateEncryptor(), CryptoStreamMode.Write, true))
            {
                var bytes = Encoding.UTF8.GetBytes(index_string);
                cs.Write(bytes);
            }
        }

        public long GetTotalSize(List<IndexedItem> indices)
        {
            long size = 0;
            foreach (var item in indices) size += item.size;
            return size;
        }

        public async Task EncryptAndAppendFile(string path, IOProgress progress)
        {
            using (FileStream fsIn = new FileStream(path, FileMode.Open))
            {
                using (CryptoStream cs = new CryptoStream(output, rijndael.CreateEncryptor(), CryptoStreamMode.Write, true))
                {
                    int read = 0;
                    while ((read = await fsIn.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await cs.WriteAsync(buffer, 0, read);
                        progress.bytesProcessed += read;
                    }
                }
            }
        }

        public void CloseAESInstance()
        {
            rijndael.Dispose();
            output.Close();
        }
    }
}
