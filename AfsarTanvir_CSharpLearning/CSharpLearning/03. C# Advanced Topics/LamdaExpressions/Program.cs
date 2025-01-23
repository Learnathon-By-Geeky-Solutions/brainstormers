// Lamda Expression : An anonymous method 'No access modifier 'No name 'no return statement
using System.Reflection;

namespace LamdaExpressions
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int x = 5;
            Console.WriteLine("Square of " + x + " is " + Square(x));

            Func<int, int> Cube = number => number * number * number;
            int y = 3;
            Console.WriteLine("Cube of " + y + " is " + Cube(y));

            const int factor = 7;
            int z = 3;
            Func<int, int> multipler = n =>
            {
                return n * factor;
            };
            Console.WriteLine("Multipler of " + z + " is " + multipler(z));


            var books = new BookRepository().GetBooks();
            var cheapBooks = books.FindAll(IsCheaperThan10Dollars);
            Console.WriteLine("Cheap Books are: ");
            foreach(var book in cheapBooks)
            {
                Console.WriteLine(book.Title);
            }

            var ExpensiveBooksLamda = books.FindAll(book => book.Price >= 10);

            Console.WriteLine("Expensive Books are: ");
            foreach (var book in ExpensiveBooksLamda)
            {
                Console.WriteLine(book.Title);
            }

            // Lambda function for product of two integers returning int64
            Func<int, int, long> Product = (a, b) => (long)a * b;

            int xx = 2000000;
            int yy = 3000000;

            Console.WriteLine("Product of " + xx + " and " + yy + " is " + Product(xx, yy));
        }
        static bool IsCheaperThan10Dollars(Book book)
        {
            return book.Price < 10;
        }
        static int Square(int x)
        {
            return x * x;
        }
    }
}
