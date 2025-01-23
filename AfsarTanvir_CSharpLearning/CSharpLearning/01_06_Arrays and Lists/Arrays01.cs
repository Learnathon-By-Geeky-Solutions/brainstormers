namespace _01_06_Arrays_and_Lists
{
    internal class Arrays01
    {
        public static void run()
        {
            int[] numbers = new int[] { 1, 3, 4, 6, 7};
            for (int i = 0; i < numbers.Length; i++)
                Console.Write(numbers[i] + " ");
            Console.WriteLine();

            //Length
            Console.WriteLine("Length of the Array : " + numbers.Length);

            //IndexOf(__arrayName, __value)
            int x = 7;
            var index = Array.IndexOf(numbers, x); // If the number not present at array, it will return -1
            Console.WriteLine("The number " + x + " at index(0 based) " + index);

            //Clear(__arrayName, __indexToStart, __HowManyToErase) 
            Array.Clear(numbers, 1, 2); // It will make those index values to 0
            for (int i = 0; i < numbers.Length; i++)
                Console.Write(numbers[i] + " ");
            Console.WriteLine();


            //Copy()
            int[] another = new int[4];
            Array.Copy(numbers, another, 4); 
            for (int i = 0; i < another.Length; i++)
                Console.Write(another[i] + " ");
            Console.WriteLine();

            //Sort()
            Array.Sort(numbers);
            for (int i = 0; i < numbers.Length; i++)
                Console.Write(numbers[i] + " ");
            Console.WriteLine();

            //Reverse()
            Array.Reverse(another);
            for (int i = 0; i < another.Length; i++)
                Console.Write(another[i] + " ");
            Console.WriteLine();
        }
    }
}
