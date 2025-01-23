namespace CSharpAdvanced.ExceptionHandling
{
    public class ExceptionHandlingStreamReader
    {
        public static void Run()
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
            catch (Exception ex)
            {
                Console.WriteLine("Sorry, " + ex);
            }
        }
    }
}
