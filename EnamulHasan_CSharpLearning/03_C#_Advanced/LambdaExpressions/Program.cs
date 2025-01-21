using System;

namespace LambdaExpress
{
    class Program
    {
        static void Main(string[] args)
        {
            // args => expression
            // number => number*number

            // () => ...
            // x => ...
            // (x, y, z) => ...

            Func<int, int> square = number => number*number;
            const int factor = 5;
            Console.WriteLine(square(5));

            Func<int, int> mul = n => n*factor;
            Console.WriteLine(mul(6));

            var books = new BookRepo().GetBooks();

            //books = books.FindAll(IsCheaperThan10Dollars);
            books = books.FindAll(b => b.Price < 10);

            foreach(var item in books){
                Console.WriteLine(item.Title);
            }
        }

        //Predicate
        static bool IsCheaperThan10Dollars(Book book){
            return book.Price < 10;
        }

        /*
        static int Square(int number){
            return number*number;
        }
        */
    }
}