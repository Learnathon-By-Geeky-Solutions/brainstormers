namespace _01_10_Debugging_Applications
{
    internal class CallStack01
    {
        public static void run()
        {
            var numbers01 = new List<int>() { 6, 5, 1, 2, 3, 4 };
            var smallests = GetSmallests(numbers01, 3);

            foreach (var smallest in smallests)
            {
                Console.WriteLine(smallest + " ");
            }
        }
        public static List<int> GetSmallests(List<int> numbers, int count)
        {
            var smallests = new List<int>();

            while (smallests.Count < count)
            {
                var min = GetSmallests(numbers);
                smallests.Add(min);
                numbers.Remove(min);
            }
            return smallests;
        }
        public static int GetSmallests(List<int> numbers)
        {
            var min = numbers[0];
            for (var i = 1; i < numbers.Count; ++i)
            {
                if (min > numbers[i])
                    min = numbers[i];
            }
            return min;
        }
    }
}
