namespace _3._Association_Between_Classes.Inheritance
{
    public class Text: PresentationObject
    {
        public int FontSize { get; set; }

        public void AddHyperLink(string url)
        {
            Console.WriteLine("We added a link to " + url);
        }
    }
}
