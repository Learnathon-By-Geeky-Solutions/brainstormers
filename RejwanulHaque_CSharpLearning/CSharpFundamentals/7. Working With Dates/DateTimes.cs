using System;

namespace _7._Working_With_Dates
{
    internal class DateTimes
    {
        public void Learn()
        {
            //var dateTime = new DateTime();
            var now = DateTime.Now;
            var today = DateTime.Today;

            Console.WriteLine($"Hour now: {now.Hour}");
            Console.WriteLine($"Minute now: {now.Minute}");

            var tomorrow = now.AddDays(1);
            var yesterday = now.AddDays(-1);

            Console.WriteLine($"Tomorrow is: {tomorrow}");
            Console.WriteLine($"Yesterday is: {yesterday}");

            Console.WriteLine($"Long Date: {now.ToLongDateString()}");
            Console.WriteLine($"Short Date: {now.ToShortDateString()}");
            Console.WriteLine($"Long Time: {now.ToLongTimeString()}");
            Console.WriteLine($"Short Time: {now.ToShortTimeString()}");
            Console.WriteLine(now.ToString("yyyy-MM-dd HH:mm"));
        }
    }
}
