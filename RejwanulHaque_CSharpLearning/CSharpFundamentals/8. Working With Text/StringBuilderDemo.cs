using System;
using System.Collections.Generic;
using System.Text;

namespace _8._Working_With_Text
{
    internal class StringBuilderDemo
    {
        public void Learn()
        {
            var builder = new StringBuilder();
            builder.Append('*', 10);
            builder.AppendLine();
            builder.Append("Hello");
            builder.AppendLine();
            builder.Append('*', 10);

            Console.WriteLine(builder);

            builder.Replace('*', '#');
            Console.WriteLine(builder);

            builder.Remove(0, 5);
            Console.WriteLine(builder);

            builder.Insert(0, new String('@', 5));
            Console.WriteLine(builder);



            var builder1 = new StringBuilder("Another String Builder");

            builder1.Append('-', 5)
                   .Append('>')
                   .AppendLine()
                   .Replace('-', '=');

            Console.WriteLine(builder1);

            Console.WriteLine($"First Character is: {builder1[0]}");

        }
    }
}
