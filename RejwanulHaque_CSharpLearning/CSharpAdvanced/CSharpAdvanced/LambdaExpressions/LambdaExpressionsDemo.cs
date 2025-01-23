using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAdvanced.LambdaExpressions
{
    internal class LambdaExpressionsDemo
    {
        public static void Learn()
        {
            Console.WriteLine(NormalFunction.Square(5));

            // lambda function
            Func<int, int> square = number => number * number;
            Console.WriteLine(square(5));

            // find all books of price less than 10 
            var books = new BooksRepository().GetBooks();
            var cheapBooks = books.FindAll(b => b.Price < 10);
            foreach (var book in cheapBooks)
            {
                Console.WriteLine(book.Title);
            }
        }

        
    }
}
