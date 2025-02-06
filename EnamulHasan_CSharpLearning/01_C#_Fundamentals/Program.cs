using System;

namespace Fundamentals
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World!");

            var john = new Person
            {
                FirstName = "John",
                LastName = "Smith"
            };
            john.Intro();

            Console.WriteLine(Calculator.Add(2, 3));

            var arr = new Arrays
            {
                numbers = new int[3]
            };
            arr.numbers[0] = 1;
            Console.WriteLine(arr.numbers[0]);

            var path = @"F:\Works_Enamul_SEC\Docs\CodeSamurai\DemoProblem\problem.pdf";
            File.Delete(path);
        }
    }
}
