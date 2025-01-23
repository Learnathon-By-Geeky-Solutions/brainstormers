namespace _01_07_Working_With_Dates
{
    internal class TimeSpan01
    {
        public static void run()
        {
            // Creating
            var timeSpan01 = new TimeSpan(1, 2, 3);
            var timeSpan02 = new TimeSpan(1, 0, 0);
            var timeSpan03 = TimeSpan.FromHours(1);

            var start = DateTime.Now;
            var end = DateTime.Now.AddMinutes(2);
            var duration = end - start;
            Console.WriteLine("Duration " + duration);

            // Properties
            Console.WriteLine("Minutes : " + timeSpan01.Minutes);
            Console.WriteLine("Total Minutes : " + timeSpan01.TotalMinutes);

            // Add
            Console.WriteLine("Add Example : " + timeSpan01.Add(TimeSpan.FromMinutes(8)));
            Console.WriteLine("Subtract Example : " + timeSpan01.Subtract(TimeSpan.FromMinutes(2)));

            // ToString
            Console.WriteLine("ToString " + timeSpan01.ToString());

            // Parse
            Console.WriteLine("Parse " + TimeSpan.Parse("01:02:03"));

            // Constructor:
            TimeSpan duration01 = new TimeSpan(days: 1, hours: 2, minutes: 30, seconds: 15);
            Console.WriteLine(duration01);  // Output: 1.02:30:15 (1 day, 2 hours, 30 minutes, 15 seconds)

            // Static Methods:
            TimeSpan duration02 = TimeSpan.FromHours(3);
            Console.WriteLine(duration02);  // Output: 03:00:00


            //  TimeSpan Properties
            TimeSpan duration03 = new TimeSpan(1, 2, 30, 15);

            Console.WriteLine("Days: " + duration03.Days);            // Output: 1
            Console.WriteLine("Hours: " + duration03.Hours);          // Output: 2
            Console.WriteLine("Minutes: " + duration03.Minutes);      // Output: 30
            Console.WriteLine("Total Hours: " + duration03.TotalHours); // Output: 26.5041666666667


            // Operations on TimeSpan
            TimeSpan duration1 = TimeSpan.FromHours(3);
            TimeSpan duration2 = TimeSpan.FromMinutes(30);
            TimeSpan total = duration1 + duration2;  // Add two TimeSpans
            Console.WriteLine(total);                // Output: 03:30:00

            DateTime st = DateTime.Now;
            DateTime en = DateTime.Now.AddHours(2).AddMinutes(15);
            TimeSpan difference = en - st;
            Console.WriteLine(difference);  // Output: 02:15:00

            // Formatting TimeSpan
            TimeSpan duration04 = new TimeSpan(1, 2, 30, 15);
            Console.WriteLine(duration04.ToString());  // Output: 1.02:30:15
            Console.WriteLine(duration04.ToString(@"hh\:mm\:ss")); // Output: 26:30:15

            // Measure Time Difference:
            start = DateTime.Now;
            // Simulate some work
            System.Threading.Thread.Sleep(3000);
            end = DateTime.Now;

            TimeSpan elapsed = end - start;
            Console.WriteLine($"Elapsed time: {elapsed.TotalSeconds} seconds");

            // Delay Execution:
            TimeSpan delay = TimeSpan.FromSeconds(5);
            Console.WriteLine("Wait for 5 seconds...");
            System.Threading.Thread.Sleep(delay);
            Console.WriteLine("Done!");

            // Schedule or Add Time:
            DateTime currentTime = DateTime.Now;
            TimeSpan interval = TimeSpan.FromHours(5);
            DateTime futureTime = currentTime.Add(interval);
            Console.WriteLine($"Future time: {futureTime}");

        }
    }
}
