using System;
using System.Collections.Generic;

namespace _2._Classes.Methods
{
    internal class ParamsDemo
    {
        public void Learn()
        {
            var numbers = new int[] { 1, 2, 3, 4, 5, 6};
            var calculator = new Calculator();
            Console.WriteLine($"Sum of 1, 2, 3 is: {calculator.add(1, 2, 3)}");
            Console.WriteLine($"Sum of numbers from 1 to 6 is: {calculator.add(numbers)}"); 
        }
    }
}
