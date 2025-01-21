using System;

namespace Fundamentals
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var john = new Person();
            john.FirstName = "John";
            john.LastName = "Smith";
            john.Intro();

            var calc = new Calculator();
            Console.WriteLine(calc.Add(2, 3));

            var arr = new Arrays();
            arr.numbers = new int[3];
            arr.numbers[0] = 1;
            Console.WriteLine(arr.numbers[0]);

            var path = @"F:\Works_Enamul_SEC\Docs\CodeSamurai\DemoProblem\problem.pdf";
            var fileobj = new Files();
            File.Delete(path);
        }
    }
}
