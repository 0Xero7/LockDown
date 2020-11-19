using System;
using System.Collections.Generic;
using System.Text;

namespace lockdown
{
    public class EncryptionPassword
    {
        private EntityDetails entity;
        public EncryptionPassword(EntityDetails entity)
        {
            this.entity = entity;
        }

        private void CenterText(string s)
        {
            Console.Write(new string(' ', (Console.WindowWidth - s.Length) / 2));
            Console.WriteLine(s);
        }

        public string RunModule()
        {
            Console.SetCursorPosition(0, 5);
            for (int i = 5; i <= 43; ++i)
            {
                Console.WriteLine(new string(' ', 50));
            }

            Console.SetCursorPosition(0, 5);
            Console.Write("          Press ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Escape");
            Console.ResetColor();
            Console.WriteLine(" to return to cancel ");

            Console.SetCursorPosition(0, 9);

            int headerSize = $"Locking {entity.shortName}".Length; 
            Console.Write(new string(' ', (Console.WindowWidth - headerSize) / 2));

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" Locking ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(entity.shortName);

            Console.ResetColor();
            Console.SetCursorPosition(0, 12);
            CenterText("Enter a strong password and remember it.");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            CenterText("If you lose  this password, you won't be");
            CenterText("able to recover the file/directory.");
            Console.ResetColor();

            Console.SetCursorPosition(0, 21);
            CenterText("Press Enter when you are done");

            Console.SetCursorPosition(5, 18);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(new string(' ', 40));
            Console.SetCursorPosition(5, 18);

            Console.ForegroundColor = ConsoleColor.Black;
            string password = "";
            bool exit = false;
            while (!exit)
            {
                var c = Console.ReadKey();
                if (Char.IsLetterOrDigit(c.KeyChar))
                {
                    Console.SetCursorPosition(5, 18);

                    password += c.KeyChar;
                    for (int i = 0; i < password.Length - 1; ++i) Console.Write('*');
                    if (password.Length > 0) Console.Write(password[^1]);
                }
                
                switch (c.Key)
                {
                    case ConsoleKey.Backspace:
                        Console.SetCursorPosition(5, 18);
                        if (password.Length == 0) break;
                        password = password.Remove(password.Length - 1);
                        for (int i = 0; i < 40; ++i)
                        {
                            if (i < password.Length) Console.Write('*');
                            else Console.Write(' ');
                        }
                        Console.SetCursorPosition(5 + password.Length, 18);
                        break;
                    case ConsoleKey.Enter:
                        return password;
                    case ConsoleKey.Escape:
                        return null;
                }
            }

            Console.ResetColor();
            return null;
        }
    }
}
