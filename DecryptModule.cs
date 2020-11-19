using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace lockdown
{
    public class DecryptModule
    {
        private async Task Decrypt(EntityDetails entity, EntityDetails writeEntity)
        {
            var pwd = new EncryptionPassword(entity);
            string password = pwd.RunModule();

            var logger = new UILogger(39);
            var decrypter = new DecrypterHelper(entity.entityName, writeEntity.entityName);

            logger.AddTask("Initializing");
            decrypter.InitializeAES(password);
            logger.TaskOK();

            logger.AddTask("Collecting Indices");
            var totalBytes = await decrypter.ReadIndices();
            logger.TaskOK();

            var progress = new IOProgress(totalBytes);
            logger.AddTask("Decrypting Files", progress: progress, type: Status.RunningWithProgressBar);
            await decrypter.DecryptAll(progress);
            logger.TaskOK();
        }

        public async Task RunModule()
        {
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var entities = IOManager.GetEntities(currentDirectory.FullName, extention: "lock");

            var ios = new UICanvas.IOSelector(
                "Select the livelock file to decrypt",
                (a) =>
                {
                    var e = IOManager.GetEntities(a.entityName, extention: "lock");
                    currentDirectory = new DirectoryInfo(a.entityName);
                    return (e, currentDirectory.FullName);
                },
                () =>
                {
                    if (currentDirectory.Parent == null) return (null, "");

                    currentDirectory = currentDirectory.Parent;
                    var e = IOManager.GetEntities(currentDirectory.FullName, extention: "lock");

                    return (e, currentDirectory.FullName);
                },
                entities,
                currentDirectory.FullName,
                allowDirectories: false
            );

            var dec = ios.GetEntity();

            entities = IOManager.GetDirectories(currentDirectory.FullName);
            var writeIOS = new UICanvas.IOSelector(
                "Select the folder to decrypt to",
                (a) =>
                {
                    var e = IOManager.GetDirectories(currentDirectory.FullName);
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

            var writeEntity = writeIOS.GetEntity();
            await Decrypt(dec, writeEntity);
        }
    }
}
