// LINQ : Language Integrated Query
// gives you the capability to query objects
// objects in memeory, eg collections (LINQ to objects)
// databases (LINQ to Entities)
// XML (LINQ to XML)
// ADO.NET Data Sets (LINQ to Data Sets)

namespace LINQ
{
    public class BookRepository
    {
        public IEnumerable<Book> GetBooks()
        {
            return new List<Book>
            {
                new Book() {Title = "Title 1", Price = 20},
                new Book() {Title = "Title 2", Price = 50},
                new Book() {Title = "Title 3", Price = 5},
                new Book() {Title = "Title 2", Price = 15},
                new Book() {Title = "Title 3", Price = 28},
                new Book() {Title = "Title 4", Price = 15},
                new Book() {Title = "Title 2", Price = 7},
                new Book() {Title = "Title 4", Price = 23},
            };
        }
    }
}
