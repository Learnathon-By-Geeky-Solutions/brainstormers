using System;

namespace LambdaExpress
{
    class Book
    {
        public string Title { get; set; }
        public int Price { get; set; }
    }

    class BookRepo
    {
        public List<Book> GetBooks()
        {
            return new List<Book>
            {
                new Book() {Title = "title1", Price = 5},
                new Book() {Title = "title2", Price = 9},
                new Book() {Title = "title3", Price = 17},
            };

        }
    }
}