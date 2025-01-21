using System;

namespace Fundamentals
{
    public class Shape
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public void Draw()
        {
            Console.WriteLine("DownCasting...");
        }
    }

    public class Circle : Shape
    {
        public decimal rad { get; set; }
    }

}

