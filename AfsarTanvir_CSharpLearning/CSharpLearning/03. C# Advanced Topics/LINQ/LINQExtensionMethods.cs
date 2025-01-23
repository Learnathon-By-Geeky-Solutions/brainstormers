// LINQ : Language Integrated Query
// gives you the capability to query objects
// objects in memeory, eg collections (LINQ to objects)
// databases (LINQ to Entities)
// XML (LINQ to XML)
// ADO.NET Data Sets (LINQ to Data Sets)

namespace LINQ
{
    public class LINQExtensionMethods
    {
        public static void Extensions01()
        {
            var book01 = new BookRepository().GetBooks();
            var cheapBooks01 = new List<Book>();
            foreach (var book in book01)
            {
                if (book.Price <= 20)
                    cheapBooks01.Add(book);
            }
            Console.WriteLine("CheapBooks List : ");
            foreach (var book in cheapBooks01)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);
            Console.WriteLine();


            // LINQ Extention Methods
            Console.WriteLine("--------LINQ Extension Methods---------");
            var book02 = new BookRepository().GetBooks();
            var cheapBooks02 = book02.Where(book => book.Price <= 20);
            Console.WriteLine("CheapBooks List Using LINQ : ");
            foreach (var book in cheapBooks02)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);
            Console.WriteLine();


            // LINQ Extention Methods
            var book03 = new BookRepository().GetBooks();
            var cheapBooks03 = book03.Where(book => book.Price < 40).OrderBy(book => book.Price); // sort by price
            //var cheapBooks03 = book03.Where(b => b.Price < 40).OrderBy(b => b.Title); // sort by title
            Console.WriteLine("CheapBooks List Using LINQ : ");
            foreach (var book in cheapBooks03)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);
            var cheapBooks03_1 = book03
                                    .Where(book => book.Price < 10)
                                    .OrderBy(book => book.Price)
                                    .Select(book => book.Title); // sort by price
            foreach (var book in cheapBooks03_1)
                Console.WriteLine("Name: " + book + ",");
            Console.WriteLine();


            // LINQ Query Operators
            Console.WriteLine("--------LINQ Query Operators---------");
            var book04 = new BookRepository().GetBooks();
            var cheapBooks04 =
                from b in book04
                where b.Price < 10
                orderby b.Title
                select b;  //select b.Title; // only title will show

            Console.WriteLine("CheapBooks List Using LINQ : ");
            foreach (var book in cheapBooks04)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);
            Console.WriteLine();
        }
        public static void Extensions02()
        {
            Console.WriteLine("Few LINQ Extension Methods");
            var books = new BookRepository().GetBooks();

            // Single
            var book01 = books.Single(book => book.Title == "Title 1");
            Console.WriteLine("Single() : " + "Name: " + book01.Title + ", Price: " + book01.Price);
            //var book011 = books.Single(book => book.Title == "Title 0"); // If book.Title match with multiple, 0 match it throw InvalidOperationException
            //Console.WriteLine("Single() : " + "Name: " + book011.Title + ", Price: " + book011.Price);

            // SingleOrDefault
            var book022 = books.SingleOrDefault(b => b.Title == "Title 1"); // If book.Title match with multiple, it throw InvalidOperationException
            Console.WriteLine("SingleOrDefault() : " + " " + ((book022 == null) ? "Null" : "Has single book, " + "Name: " + book022.Title + ", Price: " + book022.Price));
            var book02 = books.SingleOrDefault(b => b.Title == "Title 10");
            Console.WriteLine("SingleOrDefault() : " + (book02 == null ? "Null" : "Name: " + book02.Title + ", Price: " + book02.Price));

            // First
            var book03 = books.First(book => book.Title == "Title 1"); // If book.Title does not match with any, it throw InvalidOperationException
            // otherwise it return first item which match with conditions
            Console.WriteLine("First() : " + "Name: " + book03.Title + ", Price: " + book03.Price);
            var book033 = books.First(book => book.Title == "Title 3");
            Console.WriteLine("First() : " + "Name: " + book033.Title + ", Price: " + book033.Price);

            // FirstOrDefault
            var book04 = books.FirstOrDefault(book => book.Title == "Title 3"); // If book.Title does not match with any, it return null
            // otherwise it return first item which match with conditions
            Console.WriteLine("FirstOrDefault() : " + " " + ((book04 == null) ? "Null" : "Has single book, " + "Name: " + book04.Title + ", Price: " + book04.Price));
            var book044 = books.FirstOrDefault(book => book.Title == "Title 10");
            Console.WriteLine("FirstOrDefault() : " + (book044 == null ? "Null" : "Name: " + book044.Title + ", Price: " + book04.Price));

            // Last
            var book05 = books.Last(book => book.Price <= 20);
            Console.WriteLine("Last() : " + "Name: " + book05.Title + ", Price: " + book05.Price);

            // LastOrDefault
            var book06 = books.LastOrDefault(book => book.Price > 100);
            Console.WriteLine("LastOrDefault() : " + (book06 == null ? "Null" : "Name: " + book06.Title + ", Price: " + book06.Price));

            // Count
            var bookCount = books.Count(book => book.Price <= 20);
            Console.WriteLine("Count() : Number of books priced <= 20 = " + bookCount);

            // Skip and Take
            var booksSkipTake = books.Skip(2).Take(3);
            Console.WriteLine("Skip(2).Take(3) : ");
            foreach (var book in booksSkipTake)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);

            // SkipWhile and TakeWhile
            var booksSkipWhile = books.SkipWhile(book => book.Price > 5);
            Console.WriteLine("SkipWhile(Price > 15) : ");
            foreach (var book in booksSkipWhile)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);

            var booksTakeWhile = books.TakeWhile(book => book.Price <= 50);
            Console.WriteLine("TakeWhile(Price < 30) : ");
            foreach (var book in booksTakeWhile)
                Console.WriteLine("Name: " + book.Title + ", Price: " + book.Price);

            // Sum
            var totalPrice = books.Sum(book => book.Price);
            Console.WriteLine("Sum() : Total Price = " + totalPrice);

            // Max
            var maxPrice = books.Max(book => book.Price);
            Console.WriteLine("Max() : Maximum Price = " + maxPrice);

            // Min
            var minPrice = books.Min(book => book.Price);
            Console.WriteLine("Min() : Minimum Price = " + minPrice);

            // Average
            var averagePrice = books.Average(book => book.Price);
            Console.WriteLine("Average() : Average Price = " + averagePrice);
        }

    }
}
