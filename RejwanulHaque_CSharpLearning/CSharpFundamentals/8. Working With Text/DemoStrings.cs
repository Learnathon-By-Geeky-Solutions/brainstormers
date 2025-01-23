using System;

namespace _8._Working_With_Text
{
    internal class DemoStrings
    {
        public void Learn()
        {
            var fullName = "   John Smith  ";
            Console.WriteLine(fullName.Trim());
            Console.WriteLine(fullName.ToUpper());

            var index = fullName.Trim().IndexOf(' ');
            var firstName = fullName.Trim().Substring(0, index);
            var lastName = fullName.Trim().Substring(index + 1);
            Console.WriteLine($"First Name: {firstName}");
            Console.WriteLine($"Last Name: {lastName}");

            var names = fullName.Trim().Split(' ');
            Console.WriteLine($"First Name: {names[0]}");
            Console.WriteLine($"Last Name: {names[1]}");

            Console.WriteLine(fullName.Trim().Replace("John", "Rajon"));

            if(String.IsNullOrWhiteSpace("  ") )
            { Console.WriteLine("Invalid"); }

            // String to int
            var str = "25";
            var age = Convert.ToInt32(str);
            Console.WriteLine(age);

            float price = 324.34f;
            Console.WriteLine(price.ToString("C0")); // C for currency, C0 for currency(rounded)
        }
    }
}
