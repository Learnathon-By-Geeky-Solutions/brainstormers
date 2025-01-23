namespace _01_10_Debugging_Applications
{
    internal class Debugging02
    {
        public static void run()
        {
            var numbers01 = new List<int>() { 6,1,4,3,4};
            var smallests = GetSmallests(numbers01, 6);
            //var smallests = GetSmallests(null, 6);

            foreach(var smallest in smallests)
            {
                Console.Write(smallest + " ");
            }
        }
        public static List<int> GetSmallests(List<int> numbers, int count)
        {
            if(numbers == null)
                throw new ArgumentNullException("list", "The list is Null.");
            if(count > numbers.Count || count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count Should be between 1 to the size of the list.");
            }
            var list = new List<int>(numbers);
            var smallests = new List<int>();

            while (smallests.Count < count && list.Count>0)
            {
                var min = GetSmallests(list);
                smallests.Add(min);
                list.Remove(min);
            }
            return smallests;
        }
        public static int GetSmallests(List<int> numbers)
        {
            
            var min = numbers[0];
            for(var i = 1; i < numbers.Count; ++i)
            {
                if(min > numbers[i])
                    min = numbers[i];
            }
            return min;
        }
    }
}
