namespace _05_Polymorphism.Abstract_Classes_and_Members
{
    internal class AbstractClasses01
    {
        public static void run()
        {
            Console.WriteLine("\t Abstract Classes and Members\n");

            //var shape = new Shape02(); // can't create instance of a abstract class

            var circle = new Circle02();
            circle.Draw();

            var rectangle = new Rectangle02();
            rectangle.Draw();
        }
    }
}
