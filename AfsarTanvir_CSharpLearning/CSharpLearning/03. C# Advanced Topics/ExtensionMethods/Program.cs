namespace ExtensionMethods
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string post01 = "This is supposed to be a very long long blog post blah blah blah";
            var shortenedPost = post01.Shorten(5);

            Console.WriteLine("New Shorten Post : " + shortenedPost);

            IEnumerable<int> numbers = new List<int>() { 1, 5, 10, 18, 9, 15, 5};
            var mx = numbers.Max();
            Console.WriteLine("Maximum number is : " + mx);
        }
    }
}
