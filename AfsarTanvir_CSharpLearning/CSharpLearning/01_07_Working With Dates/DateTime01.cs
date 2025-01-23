namespace _01_07_Working_With_Dates
{
    internal class DateTime01
    {
        public static void run()
        {
            var dateTime = new DateTime(2015, 1, 1);
            var now = DateTime.Now;
            var today = DateTime.Today;

            Console.WriteLine(dateTime.ToString());
            Console.WriteLine(now.ToString());
            Console.WriteLine(today.ToString());
            Console.WriteLine("Hour: " + now.Hour);
            Console.WriteLine("Minute: " + now.Minute);

            var tomorrow = now.AddDays(1);
            var yesterday = now.AddDays(-1);
            Console.WriteLine(tomorrow.ToString());
            Console.WriteLine(yesterday.ToString());

            Console.WriteLine(now.ToLongDateString());
            Console.WriteLine(now.ToShortDateString());
            Console.WriteLine(now.ToLongTimeString());
            Console.WriteLine(now.ToShortTimeString());

            Console.WriteLine(now.ToString("yyyy-MM-dd"));
        }
    }
}
