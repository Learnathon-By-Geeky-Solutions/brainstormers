using System;

namespace _3._Primitive_Types___Expressions
{
    internal class TypeConversion
    {
        public void ImplicitTypeConversion()
        {
            byte b = 1;
            int i = b;
            Console.WriteLine($"{b} Converted from byte to int");

            float f = i;
            Console.WriteLine($"{f} Converted from int to float");
        }
        public void ExplicitTypeConversion()  // Casting
        {
            int i = 1;
            // byte b = i; // won't compile
            byte b = (byte)i;
            Console.WriteLine($"{b} Converted from int to byte");

            float f = 1.0f;
            i = (int)f;
            Console.WriteLine($"{i} Converted from float to int");
        }
        public void NonCompatibleTypeConversion()
        {
            string s = "23";
            int i = Convert.ToInt32(s); // ToByte(), ToInt16 etc.
            Console.WriteLine($"{i} Converted from string to int");

            int j = int.Parse(s);
            Console.WriteLine($"{j} Converted from string to int using parse");

            string sd = "2.33";
            double d = double.Parse(sd);
            Console.WriteLine($"{d} Converted from string to double using parse");
        }
    }
}