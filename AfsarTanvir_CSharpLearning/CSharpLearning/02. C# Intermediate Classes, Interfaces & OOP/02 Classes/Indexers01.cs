namespace _02_Classes
{
    public class HttpCookie
    {
        private readonly Dictionary<string, string> _dictionary;
        public DateTime Expiry { get; set; }

        public HttpCookie()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public void SetItem(string key, string value)
        {
            _dictionary[key] = value; // Add or update the value for the given key
        }

        public string GetItem(string key)
        {
            return _dictionary.ContainsKey(key) ? _dictionary[key] : null; // Return the value or null if the key doesn't exist
        }

        public string this[string key]
        {
            get { return _dictionary.ContainsKey(key) ? _dictionary[key] : null; } // Safely handle non-existing keys
            set { _dictionary[key] = value; } // Use the setter to add or update a value
        }
    }

    internal class Indexers01
    {
        public static void run()
        {
            var cookie = new HttpCookie();

            // Using the indexer to set a value
            cookie["name"] = "Mosh";

            // Using the indexer to retrieve a value
            Console.WriteLine(cookie["name"]); // Output: Mosh

            // Using the SetItem method to set a value
            cookie.SetItem("course", "C# for Beginners");

            // Using the GetItem method to retrieve a value
            Console.WriteLine(cookie.GetItem("course")); // Output: C# for Beginners

            // Trying to get a non-existing key
            Console.WriteLine(cookie.GetItem("invalidKey")); // Output: null
        }
    }
}
