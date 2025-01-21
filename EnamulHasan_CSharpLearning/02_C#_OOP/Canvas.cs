using System;

namespace Fundamentals
{
    
    public class Canvas
    {
        public void DrawShapes(List<Shape1> shapes)
        {
            foreach (var item in shapes)
            {
                item.Draw();
            }
        }
    }

}

