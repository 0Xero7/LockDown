using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;

namespace LockDown
{
    public class DecrypterHelper
    {
        private RijndaelManaged rijndael;
        private FileStream input;

        private List<IndexedItem> indices;
        private Dictionary<long, string> directories;

        private long headerLength = 0;

        byte[] buffer;
        const int sectorWindow = 1024*1024*2;
        int bufferSize;

        public DecrypterHelper(string path, string outputDir)
        {
            input = new FileStream(path, FileMode.Open, FileAccess.Read);

            directories = new Dictionary<long, string>();
            directories[-1] = outputDir;
        }

        public bool InitializeAES(string password)
        {
            rijndael = new RijndaelManaged();

            byte[] _hash = new byte[32];
            input.Read(_hash, 0, 32);
            byte[] iv = new byte[16];
            input.Read(iv, 0, 16);
            byte[] salt = new byte[32];
            input.Read(salt, 0, 32);

            var hash = new byte[32 + password.Length];
            var pwdBytes = Encoding.UTF8.GetBytes(password);
            for (int i = 0; i < pwdBytes.Length; ++i) hash[i] = pwdBytes[i];
            for (int i = 0; i < 32; ++i) hash[pwdBytes.Length + i] = salt[i];

            var hasher = SHA256.Create();
            var HASH = hasher.ComputeHash(hash);

            for (int i = 0; i < 32; ++i)
            {
                if (HASH[i] != _hash[i])
                {
                    input.Close();
                    return false;
                }
            }

            int delta = rijndael.BlockSize / 8;
            bufferSize = sectorWindow * delta;
            buffer = new byte[bufferSize];

            // Generate key
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(password, salt, 1000);
            var key = k1.GetBytes(32);

            rijndael.Mode = CipherMode.CBC;
            rijndael.Padding = PaddingMode.PKCS7;

            rijndael.Key = key;
            rijndael.IV = iv;

            return true;
        }

        private List<IndexedItem> ParseIndices(ReadOnlySpan<char> data, ref long totalBytes)
        {
            totalBytes = 0;

            var ret = new List<IndexedItem>();
            for (int i = 0; i < data.Length;)
            {
                int indexSize = int.Parse(data.Slice(i, 4).ToString());
                i += 4;

                string indexData = data.Slice(i, indexSize).ToString();
                var temp = new IndexedItem(indexData);

                if (temp.isDirectory) directories[temp.id] = directories[temp.parentID] + $@"\{temp.name}";
                temp.path = directories[temp.parentID] + $@"\{temp.name}";

                totalBytes += temp.size;
                ret.Add(temp);

                i += indexSize;
            }

            return ret;
        }

        /// <summary>
        /// Reads and stores indices and returns total bytes
        /// </summary>
        /// <returns></returns>
        public async Task<long> ReadIndices()
        {
            indices = new List<IndexedItem>();
            long totalBytes = 0;

            using (CryptoStream cs = new CryptoStream(input, rijndael.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                // Read the length of indices segment
                byte[] b_indexLength = new byte[15];
                await cs.ReadAsync(b_indexLength, 0, 15);

                int indexLength = int.Parse(Encoding.UTF8.GetString(b_indexLength));
                int delta = rijndael.BlockSize / 8;
                
                int segments = (indexLength / delta) + (indexLength % delta == 0 ? 0 : 1);
                int indexSize = segments * delta;
                byte[] b_indices = new byte[indexSize];
                await cs.ReadAsync(b_indices, 0, indexSize);

                // Why -16?
                headerLength = input.Position - 16; 

                string indexData = Encoding.UTF8.GetString(b_indices).Substring(0, indexLength);
                indices = ParseIndices(indexData.AsSpan(), ref totalBytes);
            }

            return totalBytes;
        }

        private async Task WriteFile(IndexedItem item, IOProgress progress)
        {
            int delta = rijndael.BlockSize / 8;
            //input.Seek(headerLength + (delta * item.sectorStart), SeekOrigin.Begin);
            long sectors = (item.size / delta) + 1;

            using (var fs = new FileStream(item.path, FileMode.Create, FileAccess.Write))
            {
                using (var cs = new CryptoStream(input, rijndael.CreateDecryptor(), CryptoStreamMode.Read, true))
                {
                    long limit = sectors / (long)sectorWindow;
                    long x = progress.bytesProcessed;

                    // Read all full sector windows
                    for (long i = 0; i < limit; ++i)
                    {
                        await cs.ReadAsync(buffer, 0, bufferSize);
                        await fs.WriteAsync(buffer, 0, bufferSize);

                        progress.bytesProcessed += sectorWindow / 8;
                    }

                    long remainingSize = item.size - (sectors / sectorWindow) * (delta * sectorWindow);
                    // Read remaining sectors
                    await cs.ReadAsync(buffer, 0, (int)(sectors % sectorWindow) * delta);
                    await fs.WriteAsync(buffer, 0, (int)(remainingSize));

                    progress.bytesProcessed = x + item.size;
                }
            }
        }

        public async Task DecryptAll(IOProgress progress)
        {
            foreach (var item in indices)
            {
                if (item.isDirectory) Directory.CreateDirectory(item.path);
                else await WriteFile(item, progress);
            }
        }
    }
}
