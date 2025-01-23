using _04_Non_Primitive_Types.Math;

namespace _04_Non_Primitive_Types
{
    class Program
    {
        static void Main(string[] args)
        {
            var john = new Person();
            john.FirstName = "John";
            john.LastName = "Smith";
            john.Introduction();
            Console.WriteLine("-------------------------");

            Calculator calculator = new Calculator();
            int x = 5, y = 10;
            Console.WriteLine("After adding " + x + " and " + y + " = " + calculator.Add(x,y));
            Console.WriteLine("After subtracting " + x + " and " + y + " = " + calculator.Subtract(x,y));
            Console.WriteLine("After multiping " + x + " and " + y + " = " + calculator.Multiply(x,y));
            Console.WriteLine("After dividing " + x + " and " + y + " = " + calculator.Divide(x,y));
            Console.WriteLine("-------------------------");

            Arrays01.run();
            Console.WriteLine("-------------------------");

            Strings01.run();
            Console.WriteLine("-------------------------");
            
            Enum01.run();
            Console.WriteLine("-------------------------");

            ReferenceTypesAndValueTypes01.run();
        }
    }
}
