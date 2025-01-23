using System;

namespace _7._Working_With_Dates
{
    internal class TimeSpans
    {
        public void Learn()
        {
            var timeSpan = new TimeSpan(1, 2, 4);
            Console.WriteLine($"The timespan is: {timeSpan}");
            Console.WriteLine($"The timespan is: {timeSpan.ToString()}"); // same as previous line 
            // String to timespan 
            Console.WriteLine($"Another timespan made from string: {TimeSpan.Parse("01:02:05")}");
            Console.WriteLine($"Minutes: {timeSpan.Minutes}");
            Console.WriteLine($"Total Minutes: {timeSpan.TotalMinutes}");

            var timespan1 = new TimeSpan(1, 0, 0);
            var timeSpan2 = TimeSpan.FromHours(1); // timeSpan1 and timeSpan2 are same 

            var start = DateTime.Now;
            var end = DateTime.Now.AddMinutes(10);
            var duration = end - start;
            Console.WriteLine($"Duration: {duration}");

            Console.WriteLine($"Timespan + 10 minutes: {timeSpan.Add(TimeSpan.FromMinutes(10))}");
            Console.WriteLine($"Timespan - 10 minutes: {timeSpan.Subtract(TimeSpan.FromMinutes(10))}");
        }
    }
}
