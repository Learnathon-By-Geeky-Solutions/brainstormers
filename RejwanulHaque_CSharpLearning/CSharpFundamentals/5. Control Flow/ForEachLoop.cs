using System;

namespace _5._Control_Flow
{
    public class ForEachLoop
    {
        public void PrintArray(int[] array)
        {
            foreach (var item in array)
            {
                Console.WriteLine(item);
            }
        }
    }
}
