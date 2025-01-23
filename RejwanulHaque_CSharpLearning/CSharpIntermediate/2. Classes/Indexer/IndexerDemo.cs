namespace _2._Classes.Indexer
{
    public class IndexerDemo
    {
        public static void Learn()
        {
            HttpCookie cookie = new HttpCookie();
            cookie["name"] = "John";
            Console.WriteLine(cookie["name"]);

            Console.WriteLine(cookie["ExpiryDate"]);

            cookie["otherProperty"] = "value";
            Console.WriteLine(cookie["otherProperty"]);
        }
    }
}
