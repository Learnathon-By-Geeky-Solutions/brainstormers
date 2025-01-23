namespace CSharpAdvanced.ExceptionHandling
{
    public class ExceptionHandlingYoutube()
    {
        public static void Run()
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
    }
}
