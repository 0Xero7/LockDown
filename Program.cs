using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using LockDown.UICanvas;

namespace LockDown
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);

        static async Task Main(string[] args)
        {
            var options = new List<string>()
            {
                "One", "Two", "Three"
            };

            var handle = GetStdHandle(-11);
            int mode;
            GetConsoleMode(handle, out mode);
            SetConsoleMode(handle, mode | 0x4);

            Console.WindowHeight = 45;
            Console.WindowWidth = 50;

            Console.Title = "lockdown v0.0.2.0";

            Labels.CenterLabel("lockdown v0.0.2.0", ForegroundColor: ConsoleColor.DarkGray);
            Labels.CenterLabel("authored by Soumya Pattanayak", ForegroundColor: ConsoleColor.DarkGray);
            Console.Out.Flush();

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            //var entities = IOManager.GetEntities(currentDirectory.FullName);

            //var ios = new IOSelector(
            //    (a) =>
            //    {
            //        var e = IOManager.GetEntities(a.entityName);
            //        currentDirectory = new DirectoryInfo(a.entityName);
            //        return (e, currentDirectory.FullName);
            //    },
            //    () =>
            //    {
            //        if (currentDirectory.Parent == null) return (null, "");

            //        currentDirectory = currentDirectory.Parent;
            //        var e = IOManager.GetEntities(currentDirectory.FullName);

            //        return (e, currentDirectory.FullName);
            //    },
            //    entities,
            //    currentDirectory.FullName
            //);
            //ios.GetEntity();


            //var enc = new EncryptModule();
            //await enc.RunModule();

            var dec = new DecryptModule();
            await dec.RunModule();

            //var dec = new DecrypterHelper(@"D:\test\test.txt", @"D:\extractTest");
            //await dec.ReadIndices();
            //await dec.DecryptAll();

            int x = 10;
        }
    }
}
