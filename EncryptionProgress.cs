using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lockdown
{
    public class EncryptionProgress
    {
        private bool running = false;

        public void Stop()
        {
            running = false;
        }

        public async Task RunModule()
        {
            running = true;
            Console.ResetColor();

            Console.SetCursorPosition(0, 3);
            Console.WriteLine("b\b ");
            for (int i = 3; i <= 42; ++i)
            {
                //Console.Write("b\b ");
                Console.WriteLine(new string(' ', 50));
                //Console.WriteLine("\r                                                  ");
            }

            List<string> patterns = new List<string>() { 
                "⣾",
                "⣽",
                "⣻",
                "⢿",
                "⡿",
                "⣟",
                "⣯",
                "⣷"
            };

            var progress = "\\|/-";
            int pg_ptr = 0;
            //Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (running)
            { 
                await Task.Delay(100);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write($"{progress[pg_ptr]}");
                Console.Out.Flush();
                ++pg_ptr;
                pg_ptr %= progress.Length;
            }
        }
    }
}
