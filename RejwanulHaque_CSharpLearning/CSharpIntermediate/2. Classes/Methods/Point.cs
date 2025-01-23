using System;
using System.Collections.Generic;

namespace _2._Classes.Methods
{
    internal class Point
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Move(int x, int y)
        {
            this.x += x;
            this.y += y;
        }
        public void Move(Point p)
        {
            Move(p.x, p.y);
        }
    }
}
