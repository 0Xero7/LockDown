using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown.UICanvas
{
    public class Canvas
    {
        public List<UILabel> elements;
        public Canvas(int width, int height)
        {
            Console.SetWindowSize(width, height);
            Console.ResetColor();

            elements = new List<UILabel>();
        }

        public void Render()
        {
            foreach (var item in elements) item.Render();
        }
    }
}
