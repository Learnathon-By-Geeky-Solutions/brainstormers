namespace _05_Polymorphism.Method_Overriding
{
    internal class Class01
    {
        public static void run()
        {
            Console.WriteLine("\t Method Overriding -> Class01.cs\n");
            var shapes = new List<Shape01>();
            shapes.Add(new Circle());
            shapes.Add(new Rectangle());

            Console.WriteLine("Before adding Triangle");
            var canvas01 = new Canvas();
            canvas01.DrawShapes(shapes);


            shapes.Add(new Triangle());
            Console.WriteLine("\nAfter adding Triangle");
            canvas01.DrawShapes(shapes);
        }
    }
}
