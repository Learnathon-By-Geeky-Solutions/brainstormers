namespace _03_Association_Between_Classes.Inheritance
{
    internal class Text01 : Inheritance.PresentationObject01
    {
        public int FontSize { get; set; }
        public string FontName { get; set; }

        public void AddHyperlink(string url)
        {
            Console.WriteLine("We added a like to " + url);
        }
    }
}
