namespace _01_08_Working_With_Text
{
    internal class Procedural_Programming02
    {
        public static void run()
        {
            var numbers = new List<int>();
            while (true) {
                Console.WriteLine("Enter a number (or 'Quit' to exit): ");
                var input = Console.ReadLine();
                if(input.ToLower()=="quit")
                    break;
                numbers.Add(Convert.ToInt32(input));
            }

            Console.WriteLine("Unique numbers : ");
            foreach(var number in GetUniqueNumbers(numbers))
            {
                Console.WriteLine(number);
            }
        }
        public static List<int> GetUniqueNumbers(List<int> numbers) { 
            var uniques = new List<int>();
            foreach (var number in numbers)
            { 
                if(!uniques.Contains(number))
                    uniques.Add(number);
            }
            return uniques;
        }
    }
}
