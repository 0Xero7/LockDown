using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace lockdown
{
    public class EncryptModule
    {
        private DirectoryInfo currentDirectory;

        private void Redraw()
        {
            // writes and deletes a character.
            // the "UI" isn't drawn properly unless I do this.
            Console.WriteLine("b\b ");

            Console.ResetColor();
            Console.SetCursorPosition(0, 3);
            Console.WriteLine(" ".PadLeft(50));
            Console.SetCursorPosition(0, 4);
            Console.WriteLine(" ".PadLeft(50));
            Console.SetCursorPosition(0, 5);
            Console.WriteLine(" ".PadLeft(50));
            Console.SetCursorPosition(0, 6);
            Console.WriteLine(" ".PadLeft(50));

            Console.SetCursorPosition(0, 5);
            Console.WriteLine("+---+-----+-----------------------------+--------+");
            Console.WriteLine("| T |   # | File Name                   | Size   |");
            Console.WriteLine("+---+-----+-----------------------------+--------+");

            Console.SetCursorPosition(0, 43);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.WriteLine("".PadRight(50));
            Console.ResetColor();
        }

        private void WriteOK(long ms)
        {
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\bOK");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"   [{ms} ms]\n");
            Console.ResetColor();
        }

        public async Task Encrypt(EntityDetails entity, EntityDetails saveEntity)
        {
            var enc = new EncryptionPassword(entity);
            var password = enc.RunModule();

            //var encryptor = new EncryptionProgress();
            if (password == null) Redraw();
            else
            {
                Console.SetCursorPosition(0, 5);
                //encryptor.RunModule();
            }

            Console.SetCursorPosition(0, 5);

            var helper = new EncryptionHelper(entity, saveEntity);
            var logger = new UILogger(39);

            Console.CursorVisible = false;

            logger.AddTask("Initializing");
            helper.CreateAESInstance(password);
            logger.TaskOK();




            logger.AddTask("Collecting Files");
            var timer = new Stopwatch();

            var indices = helper.GetIndices(entity.entityName);
            long totalSize = helper.GetTotalSize(indices);
            logger.TaskOK();



            logger.AddTask("Generating Indices");
            helper.GenerateFileIndices(indices);
            logger.TaskOK();



            logger.AddTask("Writing Indices");
            await helper.WriteIndices("", indices);
            logger.TaskOK();



            int filesDone = 0;
            IOProgress progress = new IOProgress(totalSize);
            logger.AddTask("Encrypting files", Status.RunningWithProgressBar, progress);
            foreach (IndexedItem s in indices)
            {
                if (!s.isDirectory)
                {
                    await helper.EncryptAndAppendFile(s.path, progress);
                }

                ++filesDone;
                logger.UpdateTask(s.name.PadRight(50), (int)((progress.bytesProcessed * 100) / totalSize));
            }
            logger.TaskOK();

            helper.CloseAESInstance();

            Console.CursorVisible = true;
        }

        public async Task RunModule()
        {
            currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var entities = IOManager.GetEntities(currentDirectory.FullName);

            var ios = new UICanvas.IOSelector(
                "Select the file/folder to encrypt",
                (a) =>
                {
                    var e = IOManager.GetEntities(a.entityName);
                    currentDirectory = new DirectoryInfo(a.entityName);
                    return (e, currentDirectory.FullName);
                },
                () =>
                {
                    if (currentDirectory.Parent == null) return (null, "");

                    currentDirectory = currentDirectory.Parent;
                    var e = IOManager.GetEntities(currentDirectory.FullName);

                    return (e, currentDirectory.FullName);
                },
                entities,
                currentDirectory.FullName
            );

            var selection = ios.GetEntity();
            if (selection == null) return;

            currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            entities = IOManager.GetDirectories(currentDirectory.FullName);

            var saveIOS = new UICanvas.IOSelector(
                "Select the file/folder to save to",
                (a) =>
                {
                    var e = IOManager.GetDirectories(a.entityName);
                    currentDirectory = new DirectoryInfo(a.entityName);
                    return (e, currentDirectory.FullName);
                },
                () =>
                {
                    if (currentDirectory.Parent == null) return (null, "");

                    currentDirectory = currentDirectory.Parent;
                    var e = IOManager.GetDirectories(currentDirectory.FullName);

                    return (e, currentDirectory.FullName);
                },
                entities,
                currentDirectory.FullName
            );

            var savePlace = saveIOS.GetEntity();
            await Encrypt(selection, savePlace);
        }
    }
}
