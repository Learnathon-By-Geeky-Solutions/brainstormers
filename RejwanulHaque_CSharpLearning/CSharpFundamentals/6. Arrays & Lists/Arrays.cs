using System;

namespace _6._Arrays___Lists
{
    internal class Arrays
    {
        public void PrintArray(int[] array, string message)
        {
            Console.Write(message);
            foreach (var item in array)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }
        public void Learn()
        {
            var numbers = new[] { 2, 1, 2, 4, 3, 9, 5, 8 };
            PrintArray(numbers, "The array is: ");
            Console.WriteLine($"Length: {numbers.Length}");

            var index = Array.IndexOf(numbers, 3);
            Console.WriteLine($"Index of 3 is: {index}");

            Array.Clear(numbers, 3, 2); // set numbers[3] = 0 and numbers[4] = 0
            PrintArray(numbers, "The array after clearing 2 elements from index 3: ");

            var another = new int[3];
            // Array.Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);
            Array.Copy(numbers, 2, another, 0, 3);
            PrintArray(another, "The copied array is: ");

            Array.Sort(numbers);
            PrintArray(numbers, "Sorted array: ");

            Array.Reverse(numbers);
            PrintArray(numbers, "Reversed array: ");
        }
    }
}
