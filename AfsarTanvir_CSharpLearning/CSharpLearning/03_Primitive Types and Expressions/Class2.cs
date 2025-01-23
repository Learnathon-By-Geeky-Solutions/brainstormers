namespace _03_Primitive_Types_and_Expressions
{
    internal class Class2
    {
        public static void run()
        {
            // Implicit Type Coversion
            int i1 = 1;
            float f1 = i1;
            Console.WriteLine("Implicit Type Conversion, int = " + i1 + ", float = " + f1);

            // Explicit Type Conversion
            // Compiler does not allow it, we use casting to allow
            float f2 = 1.0f;
            int i2 = (int) f2;
            Console.WriteLine("Explicit Type Conversion, float = " + f2.ToString("0.00") + ", int = " + i2);

            // Non-compatible types
            string s = "1";
            int i = Convert.ToInt32(s);
            int j = int.Parse(s);
            Console.WriteLine("string s = " + s+s + ", int i = " + (i+i) + ", int j = " + j+j);

            try
            {
                var number = "12345";
                byte b = Convert.ToByte(number);
                Console.WriteLine("After Converting the number " + number);
                Console.WriteLine("To byte - ", b);
            }
            catch (Exception)
            {
                Console.WriteLine("The Number can not be conterted.");
            }

            i = 15; j = 2;
            Console.WriteLine(i / (float)j);
        }
    }
}
