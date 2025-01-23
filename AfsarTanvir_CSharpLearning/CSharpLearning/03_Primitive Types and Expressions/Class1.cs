namespace _03_Primitive_Types_and_Expressions
{
    internal class Class1
    {
        public static void run()
        {
            byte number = 1;
            int count = 10;
            float totalPrice = 20.54f;
            Console.WriteLine("Number " + number + ", count = " + count + " Total Price = " + totalPrice);
            var myName = "Afsar";
            Console.WriteLine(myName);
            Console.WriteLine("Byte Min Value : {0},\nByte Max Value : {1}", byte.MinValue, byte.MaxValue);
            Console.WriteLine("Float Min Value : {0},\nDouble Max Value : {1}", float.MinValue, double.MaxValue);
        }
    }
}
