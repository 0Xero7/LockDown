using System;
using System.Collections.Generic;
using System.Text;

namespace lockdown.UICanvas
{
    public class IOSelector
    {
        private List<EntityDetails> entities;
        private Func<EntityDetails, (List<EntityDetails>, string)> GoInto;
        private Func<(List<EntityDetails>, string)> GoUp;
        private int index = 0;
        private string currentDirectory;
        private bool allowDirectories;

        private bool directoryDirty = true;

        private string Title;

        private const int maxLines = 34;
        private int topIndex = 0;

        public IOSelector(
            string Title,
            Func<EntityDetails, (List<EntityDetails>, string)> GoInto,
            Func<(List<EntityDetails>, string)> GoUp,
            List<EntityDetails> entities, string currentDirectory,
            string extension = null,
            bool allowDirectories = true)
        {
            this.Title = Title;

            this.GoInto = GoInto;
            this.GoUp = GoUp;

            this.entities = entities;
            this.currentDirectory = currentDirectory;
            this.directoryDirty = true;

            this.allowDirectories = allowDirectories;
        }

        public EntityDetails GetEntity()
        {
            Console.SetCursorPosition(0, 3);
            Labels.CenterLabel($" {Title} ", BackgroundColor: ConsoleColor.DarkGray);

            Console.SetCursorPosition(0, 44);
            Console.Write("[G]Select [↑/↓]Move [←/→]Change Directory");

            while (true)
            {
                Render();
                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        ++index;
                        break;
                    case ConsoleKey.UpArrow:
                        --index;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Enter:
                        if (!entities[index].isDirectory) break;

                        var (newEntities, cd) = GoInto(entities[index]);
                        if (newEntities == null) break;

                        this.directoryDirty = true;
                        entities = newEntities;
                        currentDirectory = cd;
                        index = 0;
                        Render();
                        break;
                    case ConsoleKey.LeftArrow:
                        var (_newEntities, _cd) = GoUp();
                        if (_newEntities == null) break;

                        this.directoryDirty = true;
                        entities = _newEntities;
                        currentDirectory = _cd;
                        index = 0;
                        Render();
                        break;
                    case ConsoleKey.G:
                        if (entities[index].isDirectory && !allowDirectories) break;
                        return entities[index];
                }

                index = (index + entities.Count) % Math.Max(entities.Count, 1);
                if (index < topIndex)
                {
                    topIndex = index;
                }
                if (index > topIndex + maxLines - 1)
                {
                    topIndex = index - maxLines + 1;
                }
            }

            // TODO: Add a no select option
            return null;
        }

        private void DrawDirectoryTabs()
        {
            var dirs = currentDirectory.Split("\\");
            Console.SetCursorPosition(0, 5);

            Console.Write(new string(' ', 50));
            Console.SetCursorPosition(0, 5);

            if (dirs.Length == 2 && dirs[^1] == "")
            {
                ConsoleUtils.WriteRGBAndFG(83, 53, 65, 255, 255, 255, $" {dirs[0]} ");
                ConsoleUtils.WriteRGBAndFG(83, 53, 65, 20, 20, 20, $@"\");
            }
            else
            {
                int remaining = 50 - 5;  // Drive letter and slash will always be there
                ConsoleUtils.WriteRGBAndFG(83, 53, 65, 255, 255, 255, $" {dirs[0]} ");
                ConsoleUtils.WriteRGBAndFG(83, 53, 65, 20, 20, 20, $@"\");

                // Can we add the current directory name
                if (remaining - dirs[^1].Length - 2 < 0)
                {
                    ConsoleUtils.WriteRGBAndFG(83, 53, 65, 255, 255, 255, $" {dirs[^1].Substring(0, remaining - 5)}... ");
                }
                else
                {
                    List<string> path = new List<string>();
                    path.Add(dirs[0]);
                    remaining -= dirs[^1].Length;
                    remaining -= 2;

                    bool canFillAll = true;
                    for (int i = dirs.Length - 2; i >= 1; --i)
                    {
                        if (dirs[i].Length + 3 <= remaining)
                        {
                            path.Add($"{dirs[i]}");
                            remaining -= dirs.Length + 3;
                        } else
                        {
                            canFillAll = false;
                            break;
                        }
                    }
                    path.Reverse();
                    path.RemoveAt(path.Count - 1);

                    if (!canFillAll)
                    {
                        ConsoleUtils.WriteRGBAndFG(73, 43, 55, 255, 255, 255, $" ... ");
                        ConsoleUtils.WriteRGBAndFG(73, 43, 55, 20, 20, 20, $@"\");
                    }
                    for (int i = 0; i < path.Count; ++i)
                    {
                        ConsoleUtils.WriteRGBAndFG(73, 43, 55, 255, 255, 255, $" {path[i]} ");
                        ConsoleUtils.WriteRGBAndFG(73, 43, 55, 20, 20, 20, $@"\");
                    }

                    if (dirs.Length > 1)
                    {
                        ConsoleUtils.WriteRGBAndFG(83, 53, 65, 255, 255, 255, $" {dirs[^1]} ");
                    }
                }

                Console.ResetColor();
            }
        }

        private void Render()
        {
            int len = entities.Count;

            if (directoryDirty)
            {
                DrawDirectoryTabs();
                directoryDirty = false;
            }

            Console.SetCursorPosition(0, 6);
            Console.WriteLine("┌───┬─────┬─────────────────────────────┬────────┐");
            Console.WriteLine("│ T │   # │ File Name                   │ Size   │");
            Console.WriteLine("├───┼─────┼─────────────────────────────┼────────┤");
            Console.SetCursorPosition(0, 9);
            for (int i = topIndex; i <= topIndex + 33; ++i)
            {
                if (i < len)
                {
                    bool isDirectory = entities[i].isDirectory;
                    int idx = i + 1;
                    string fileName = entities[i].shortName;
                    if (fileName.Length > 27) fileName = fileName.Substring(0, 24) + "...";
                    fileName = fileName.PadRight(27);
                    string size = (isDirectory ? "" : entities[i].formattedSize);

                    string line = " ";
                    Console.Write("│");
                    if (isDirectory) line += "D";
                    else line += " ";
                    line += " │ " + (idx.ToString()).PadLeft(3) + " │ " + fileName + " │ " + size.PadLeft(6) + " ";

                    if (i == index)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                    }
                    Console.Write(line);
                    if (i == index) { Console.ResetColor(); }
                    Console.WriteLine("│");
                }
                else
                {
                    Console.WriteLine("│   │     │                             │        │");
                }
            }
            Console.WriteLine("└───┴─────┴─────────────────────────────┴────────┘");
        }
    }
}
