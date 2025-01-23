// Lamda Expression : An anonymous method 'No access modifier 'No name 'no return statement
namespace LamdaExpressions
{
    public class BookRepository
    {
        public List<Book> GetBooks()
        {
            return new List<Book>
            {
                new Book() {Title = "Title 1", Price = 5},
                new Book() {Title = "Title 2", Price = 16},
                new Book() {Title = "Title 3", Price = 9}
            };
        }
    }
}
