using System;

namespace _7._Working_With_Dates
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DateTimes: ");
            DateTimes dateTimes = new DateTimes();
            dateTimes.Learn();

            Console.WriteLine();
            Console.WriteLine("TimeSpans: ");

            TimeSpans timeSpans = new TimeSpans();
            timeSpans.Learn();
        }
    }
}
