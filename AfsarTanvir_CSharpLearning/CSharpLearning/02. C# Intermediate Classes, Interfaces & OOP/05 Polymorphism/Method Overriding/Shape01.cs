namespace _05_Polymorphism.Method_Overriding
{
    public class Circle : Shape01
    {
        public override void Draw()
        {
            Console.WriteLine("Draw a Circle");
        }
    }
    public class Rectangle : Shape01
    {
        public override void Draw()
        {
            Console.WriteLine("Draw a Rectangle");
        }
    }
    public class Triangle : Shape01
    {
        public override void Draw()
        {
            Console.WriteLine("Draw a Triangle");
        }
    }
    public class Shape01
    {
        public int Width { get; set; }
        public int Height { get; set; }
        // public Position Position { get; set; }
        public virtual void Draw()
        {

        }
    }
}
