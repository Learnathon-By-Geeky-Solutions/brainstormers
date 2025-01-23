using System.Reflection;
using System;
using System.Collections.Generic;

namespace Generics
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var book = new Book { Isbn = "1111", Title = "C# Advance" };

            var numbers = new List<int>();
            numbers.Add(1);

            //var books01 = new BookList();
            //books01.Add(book);
            //var numbers02 = new GenericList<int>();
            //numbers02.Add(1);

            var books = new GenericList<Book>();
            books.Add(new Book());

            var dictionary = new GenericDictionary<string, Book>();
            dictionary.Add("1234", new Book());

            var number01 = new Nullable<int>(5);
            Console.WriteLine("Has Value : " + number01.HasValue);
            Console.WriteLine("Value " + number01.GetValueOrDefault());
            
            var number02 = new Nullable<int>(0);
            Console.WriteLine("Has Value : " + number02.HasValue);
            Console.WriteLine("Value " + number02.GetValueOrDefault());
        }
    }
}
