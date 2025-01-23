namespace CSharpAdvanced.LambdaExpressions
{
    public class BooksRepository
    {
        public List<Book> GetBooks()
        {
            return new List<Book>
            {
                new Book() {Title = "book1", Price = 4 },
                new Book() {Title = "book2", Price = 14 },
                new Book() {Title = "book3", Price = 9 },
                new Book() {Title = "book4", Price = 10 }
            };
        }
    }
}
