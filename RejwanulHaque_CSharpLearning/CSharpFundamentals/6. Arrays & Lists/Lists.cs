using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6._Arrays___Lists
{
    internal class Lists
    {
        public void PrintList(List <int> list, string message)
        {
            Console.Write(message);
            foreach(var item in list)
            {
                Console.Write($"{item} ");
            }
        }
        public void Learn()
        {
            var numbers = new List<int>() { 1, 5, 3, 3, 2, 6};
            numbers.Add(1);

            numbers.AddRange(new[] { 1, 5, 5});

            PrintList(numbers, "The List is: ");

            Console.WriteLine($"Index of 1 is {numbers.IndexOf(1)}");
            Console.WriteLine($"Last Index of 1 is {numbers.LastIndexOf(1)}");
            Console.WriteLine($"Index of 555 is {numbers.IndexOf(555)}"); // -1 (not found) 
            Console.WriteLine($"Number of elements: {numbers.Count}");

            numbers.Remove(1);
            PrintList(numbers, "After removing 1: "); // removed the first 1 
            Console.WriteLine($"Number of elements after removing 1: {numbers.Count}");

            // remove all 5's

            //foreach(var item in numbers )
            //{
            //    if (item == 5) numbers.Remove(item);  // exception occurs due to removing inside foreach loop 
            //}

            for(var i=numbers.Count-1; i>=0; i--) // Mosh's lecture does a mistake here, he iterates from 0 to numbers.Count-1, by which some indexes got skipped 
            {
                if(numbers[i] == 5) numbers.Remove(numbers[i]);
            }
            PrintList(numbers, "After removing all 5's: ");
        }
    }
}
