using System;

namespace _3._Primitive_Types___Expressions
{
    internal class VariablesAndContants
    {
        public void DataTypes()
        {
            byte b = 2;
            int i = 4;
            float f = 1.4f; // by default double 
            char c = 'A';
            string s = "String";
            bool isOk = false;
            var vstring = "var";
            var vint = 6;

            Console.WriteLine($"{b} is byte");
            Console.WriteLine($"{i} is int");
            Console.WriteLine($"{f} is float");
            Console.WriteLine($"{c} is char");
            Console.WriteLine($"{s} is string");
            Console.WriteLine($"{isOk} is boolean");
            Console.WriteLine($"{vstring} is a string declared with var");
            Console.WriteLine($"{vint} is an int declared with var");

            // range
            Console.WriteLine($"Min and Max values of byte are {byte.MinValue} and {byte.MaxValue}");
        }
    }
}