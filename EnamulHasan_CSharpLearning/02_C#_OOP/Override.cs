using System;

namespace Fundamentals
{
    
    public struct Position {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Rectangle : Shape1
    {
        public override void Draw()
        {
            Console.WriteLine("Draw rectangle");
        }
    }

    public class Circle1 : Shape1
    {
        public override void Draw()
        {
            Console.WriteLine("Draw circle");
        }
    }

    public class Triangle : Shape1
    {
        public override void Draw()
        {
            Console.WriteLine("Draw triangle");
        }
    }

    public class Shape1
    {
       
        public int Width { get; set; }
        public int Height { get; set; }
        public Position Position { get; set; }

        public virtual void Draw()
        {
            Console.WriteLine("");
        }
    }
}

