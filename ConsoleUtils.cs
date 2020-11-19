using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown
{
    public static class ConsoleUtils
    {
        public static void WriteWithFBG(int fgIdx, int bgIdx, string s)
        {
            //Console.Write("\x1b[48;5;" + bgIdx + $"m\x1b[38;5;" + fgIdx + $"m{s}");
            string x = $"\x1b[48;2;100;200;80m{s}";
            Console.Write(x);
            //Console.Write("\x1b[48;2;" + bgIdx + $"m\x1b[38;5;" + fgIdx + $"m{s}");
        }

        public static void WriteRGB(int r, int g, int b, string s)
        {
            string x = $"\x1b[48;2;{r};{g};{b}m{s}";
            Console.Write(x);
        }

        public static void WriteRGBAndFG(int r, int g, int b, int rx, int gx, int bx, string s)
        {
            string x = $"\x1b[48;2;{r};{g};{b}m\x1b[38;2;{rx};{gx};{bx}m{s}";
            Console.Write(x);
        }
    }
}
