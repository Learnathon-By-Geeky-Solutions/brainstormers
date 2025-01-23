namespace ExceptionHandling
{
    internal class Program
    {
        public static void Run1()
        {
            try
            {
                var calculator = new Calculator01();

                // Uncomment one case at a time to trigger specific exceptions.

                // Case 1: Divide by zero (DivideByZeroException)
                 var result = calculator.Divide(5, 0);

                // Case 2: Invalid argument (ArgumentException)
                // var result = calculator.Divide(-5, 2);

                // Case 3: No exception
                //var result = calculator.Divide(10, 2);

                // Case 4: Unexpected exception (simulate any other error)
                // throw new Exception("Unexpected error occurred.");


                Console.WriteLine($"Result: {result}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine("Caught DivideByZeroException: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Caught ArgumentException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught General Exception: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Execution of Run1 completed. Cleaning up resources if any.");
            }
        }

        public static void streamReaderException2()
        {
            try
            {
                //using (var streamReader = new StreamReader(@"C:\Users\t440s\Downloads\OneDrive_2025-01-07.zip")) // Successfully Opened.
                //using (var streamReader = new StreamReader(@"C:\Users\t440s\Downloads\Emulator 8086 1.zip")) // Sorry, System.IO.FileNotFoundException
                using (var streamReader = new StreamReader(@"C:\Users\t440s\Downloads\Emulator 8086.zip")) // Successfully Opened.
                {
                    var content = streamReader.ReadToEnd();
                    Console.WriteLine("Successfully Opened.");
                }            
            }
            catch(Exception ex)
            {
                Console.WriteLine("Sorry, " + ex);
            }
        }
        public static void YouTube3()
        {
            try
            {
                var api = new YouTubeApi();
                var videos = api.GetVideos("Afsar Tanvir");
            }
            catch (YouTubeException ex)
            {
                Console.WriteLine("Caught YouTubeException: " + ex.Message);
                Console.WriteLine("Inner Exception: " + ex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught General Exception: " + ex.Message);
            }
        }
        static void Main(string[] args)
        {
            //Run1();
            //streamReaderException2();
            YouTube3();
        }
    }
}
