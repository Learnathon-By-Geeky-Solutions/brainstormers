using System.Text;

namespace _01_08_Working_With_Text
{
    internal class StringBuilder01
    {
        public static void run()
        {
            var builder01 = new StringBuilder();
            builder01.Append('*', 10);
            builder01.AppendLine();
            builder01.Append("Header");

            builder01.Replace('*', '+');

            Console.WriteLine(builder01);
            Console.WriteLine();
            
            var builder02 = new StringBuilder(" Hello World ");
            Console.WriteLine(builder02);

            builder02.Append('-', 10)
                .AppendLine()
                .Append("Afsar")
                .Replace('l', '1')
                .Insert(0, new string('+', 10));

            Console.WriteLine(builder02);
            Console.WriteLine("First Char : " + builder02[0]);
        }
    }
}
