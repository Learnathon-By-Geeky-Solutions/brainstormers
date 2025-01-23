namespace _01_08_Working_With_Text
{
    // Procedural Programming -> A programming paradigm based on procedural calls
    // OOP -> A programming paradigm based on objects
    internal class Program
    {
        static void Main(string[] args)
        {
            //String01.run();
            var str01 = "My name is Sayed Hossen Afsar Tanvir. What's your name?";
            var str02 = Summarising01.SummerizeText(str01, 20);
            Console.WriteLine(str02);
            Console.WriteLine("-----------------------");

            //StringBuilder01.run();
            Console.WriteLine("-----------------------");

            //Procedural_Programming01.run();
            Console.WriteLine("-----------------------");

            Procedural_Programming02.run();

        }
    }
}
