using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAdvanced.Generics
{
    internal class GenericsDemo
    {
        public static void Learn()
        {
            var book = new Book { Isbn = "1111", Title = "C# Advanced" };

            var numbers = new GenericsList<int>();
            numbers.Add(1);

            var books = new GenericsList<Book>();
            books.Add(book);

            var dictionary = new GenericDictionary<string, Book>();
            dictionary.Add("1234", new Book());


            var number = new Nullable<int>(5);
            Console.WriteLine("Has Value? "+number.HasValue);
            Console.WriteLine("Value: "+number.GetValueOrDefault());

            var number1 = new Nullable<int>();
            Console.WriteLine("Has Value? " + number1.HasValue);
            Console.WriteLine("Value: " + number1.GetValueOrDefault());
        }
    }
}
