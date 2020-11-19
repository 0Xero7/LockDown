using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown.UICanvas
{
    public static class Labels
    {
        public static void CenterLabel(string s, bool stay = false,
            ConsoleColor ForegroundColor = ConsoleColor.White,
            ConsoleColor BackgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.Write(s);
            if (!stay) Console.WriteLine();
            Console.ResetColor();
        }
    }
}
