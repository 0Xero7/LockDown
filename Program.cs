using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using lockdown.UICanvas;

namespace lockdown
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
            var handle = GetStdHandle(-11);
            int mode;
            GetConsoleMode(handle, out mode);
            SetConsoleMode(handle, mode | 0x4);

            Console.WindowHeight = 45;
            Console.WindowWidth = 50;

            Console.Title = "lockdown v0.0.2.1";

            Labels.CenterLabel("lockdown v0.0.2.1", ForegroundColor: ConsoleColor.DarkGray);
            Labels.CenterLabel("authored by Soumya Pattanayak", ForegroundColor: ConsoleColor.DarkGray);
            Console.Out.Flush();

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var choices = new List<string>()
            {
                "Encrypt",
                "Decrypt"
            };

            var ch = new UIChoice(choices, "What do you want to do?");

            if (ch.GetChoice() == 0)
            {
                var enc = new EncryptModule();
                await enc.RunModule();
            }
            else
            {
                var dec = new DecryptModule();
                await dec.RunModule();
            }
        }
    }
}
