namespace _01_06_Arrays_and_Lists
{
    internal class Lists01
    {
        public static void run()
        {
            var list01 = new List<int>() { 1, 2, 3, 4};
            list01.Add(6);
            list01.AddRange(new int[] {5, 1, 7, 9});

            foreach(var i in list01)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            int x = 1;
            Console.WriteLine("At index " + x + " value is " + list01.IndexOf(x)); // if the value not present in list, then it will return -1
            Console.WriteLine("At index " + x + " value is " + list01.LastIndexOf(x)); // if the value not present in list, then it will return -1

            Console.WriteLine("Count : " + list01.Count);
            //list01.Remove(x);
            //foreach (var i in list01)
            //{
            //    Console.Write(i + " ");
            //}
            //Console.WriteLine();

            for (var i=0; i<list01.Count; ++i)
            {
                if (list01[i] == x)
                {
                    list01.Remove(list01[i]);
                }
            }
            foreach (var i in list01)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();

            Console.WriteLine("Count : " + list01.Count);
            // remove all elements
            list01.Clear(); 
            Console.WriteLine("Count : " + list01.Count);

        }
    }
}
