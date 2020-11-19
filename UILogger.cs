using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LockDown
{
    public enum Status
    {
        RunningWithProgressBar,
        Running,
        OK,
        Fail
    };

    public class UILogger
    {
        private List<string> lines;
        private List<Status> status;
        private List<IOProgress> progressBars;
        public int maxLines;

        private DateTime lastRender;
        private bool rendering = false;

        private int progressTaskProgress = 0;

        string[] progress = new string[12]
            {
                "▌   ",
                "██  ",
                " ██ ",
                "  ██",
                "   ▐",
                "   ▐",
                "   ▐",
                "  ██",
                " ██ ",
                "██  ",
                "▌   ",
                "▌   ",
            };
        private int ptr = 0;

        public UILogger(int maxLines)
        {
            this.maxLines = maxLines;
            lines = new List<string>();
            status = new List<Status>();
            progressBars = new List<IOProgress>();

            Console.ResetColor();

            Render();

            lastRender = DateTime.Now;
            ptr = 0;

            NextFrame();
        }

        private async Task NextFrame()
        {
            while (true)
            {
                Render();
                await Task.Delay(32); // 30 FPS
            }
        }

        public void AddTask(string line, Status type = Status.Running, IOProgress progress = null)
        {
            progressTaskProgress = 0;
            lines.Add(line);
            status.Add(type);
            progressBars.Add(progress);
        }

        public void UpdateTask(string s, int progress)
        {
            lines[^1] = s;
            progressTaskProgress = progress;
        }

        public void TaskOK()
        {
            status[^1] = Status.OK;
        }

        private void Render(bool force = false)
        {
            rendering = true;

            // Clear the screen
            Console.SetCursorPosition(0, 5);
            for (int i = 5; i <= 42; ++i)
            {
                Console.WriteLine(new string(' ', 50));
            }

            bool anyRunning = false;
            int line = 0;
            for (int i = Math.Max(0, lines.Count - maxLines); i < lines.Count; ++i)
            {
                Console.SetCursorPosition(1, 5 + line);
                switch (status[i])
                {
                    case Status.RunningWithProgressBar:
                        Console.SetCursorPosition(6, 5 + Math.Min(maxLines, lines.Count));

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(new string('░', 38));
                        
                        Console.ForegroundColor = ConsoleColor.Gray;
                        int bars = (int)(38 * progressBars[i].bytesProcessed / progressBars[i].totalBytes);
                        Console.SetCursorPosition(6, 5 + Math.Min(maxLines, lines.Count));
                        Console.Write(new string('█', bars));
                        Console.ResetColor();

                        Console.SetCursorPosition(44, 5 + Math.Min(maxLines, lines.Count));
                        Console.Write($"{progressBars[i].bytesProcessed * 100 / progressBars[i].totalBytes}%".PadLeft(5));


                        if (lines.Count > 0)
                        {
                            anyRunning = true;

                            Console.SetCursorPosition(1, 4 + Math.Min(maxLines, i + 1));
                            Console.Write($"{progress[ptr]}");
                        }
                        break;
                    case Status.Running:
                        if (lines.Count > 0)
                        {
                            anyRunning = true;

                            Console.SetCursorPosition(1, 4 + Math.Min(maxLines, i + 1));
                            Console.Write($"{progress[ptr]}");
                            ptr %= progress.Length;
                        }
                        break;
                    case Status.OK:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" OK  ");
                        Console.ResetColor();
                        break;
                }

                Console.SetCursorPosition(6, 5 + line);
                Console.WriteLine(lines[i]);
                ++line;
            }

            if (anyRunning)
            {
                ++ptr;
                ptr %= progress.Length;
            }

            rendering = false;
        }
    }
}
