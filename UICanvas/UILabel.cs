using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown.UICanvas
{
    public class UILabel
    {
        public string label { get; set; }
        public bool center { get; set; }

        public int posX { get; set; }
        public int posY { get; set; }

        public UILabel(string label, bool center = false, int posX = 0, int posY = 0)
        {
            this.label = label;
            this.center = center;
            this.posX = posX;
            this.posY = posY;
        }

        public void Render()
        {
            if (center)
            {
                Console.SetCursorPosition((Console.WindowWidth - label.Length) / 2, posY);
            } else
            {
                Console.SetCursorPosition(posX, posY);
            }
            Console.Write(label);
        }
    }
}
