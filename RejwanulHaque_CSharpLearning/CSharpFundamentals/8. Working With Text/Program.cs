using System;

namespace _8._Working_With_Text
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DemoStrings demoStrings = new DemoStrings();
            demoStrings.Learn();

            Console.WriteLine();
            Console.WriteLine("StringUtility");

            var sentence = "This is a very long sentence. I want to show just the first few words. ";
            Console.WriteLine(StringUtility.summarizeText(sentence, 20));

            Console.WriteLine();
            Console.WriteLine("StringBuilderDemo: ");

            StringBuilderDemo stringBuilderDemo = new StringBuilderDemo(); 
            stringBuilderDemo.Learn();
        }
    }
}
