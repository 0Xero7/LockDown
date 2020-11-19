using System;
using System.Collections.Generic;
using System.Text;

namespace lockdown.UICanvas
{
    public class UIChoice
    {
        private List<string> choices;
        private int index = 0;

        private string Title;

        private int topLine = 0;
        private int gap = 4;

        public UIChoice(List<string> choices, string title)
        {
            this.choices = choices;
            this.Title = title;
            topLine = (Console.WindowHeight - choices.Count - gap) / 2;
        }

        public int GetChoice()
        {
            Console.SetCursorPosition(0, topLine);
            Labels.CenterLabel(Title);

            while (true)
            {
                Render();

                var key = Console.ReadKey(true);
                switch(key.Key)
                {
                    case ConsoleKey.Enter:
                        return index;
                    case ConsoleKey.UpArrow:
                        --index;
                        break;
                    case ConsoleKey.DownArrow:
                        ++index;
                        break;
                }

                index = (index + choices.Count) % choices.Count;
            }
        }

        private void Render()
        {
            Console.SetCursorPosition(0, topLine + gap);

            for (int i = 0; i < choices.Count; ++i)
            {
                if (i == index) Labels.CenterLabel($" {choices[i]} ", BackgroundColor: ConsoleColor.DarkGray);
                else Labels.CenterLabel($" {choices[i]} ");
            }
        }
    }
}
