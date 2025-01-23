// Events -> an event is a special kind of delegate that is used to notify
// other objects when something happens. Events provide a way for an object to
// communicate changes or actions to other objects without tightly coupling them.
// Benefits : Enables loosely coupled communication, Promotes modularity,
//            Simplifies maintaining and scaling the codebase.
using System.Text;

namespace Events
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var video01 = new Video() { Title = "Video 1" };
            var videoEncoder01 = new VideoEncoder();
            videoEncoder01.Encode(video01);
            Console.WriteLine();

            var video02 = new Video() { Title = "Video 2" };
            var videoEncoder02 = new VideoEncoder(); // publisher
            var mailServer = new MailServer(); // subscriber

            videoEncoder02.VideoEncoded += mailServer.onVideoEncoded;
            videoEncoder02.EventEncode(video02); // Raise event

            var messageService = new MessageServics(); // subscriber
            videoEncoder02.VideoEncoded += messageService.onVideoEncoded;
            videoEncoder02.EventEncode(video02);
            Console.WriteLine();

            var video03 = new Video() { Title = "Video 3" };
            var videoEncoder03 = new VideoEncoder();
            var mailServer03 = new MailServer();
            var messageService03 = new MessageServics();

            videoEncoder03.VideoEncoded03 += mailServer.onVideoEncoded;
            videoEncoder03.VideoEncoded03 += messageService.onVideoEncoded;
            videoEncoder03.EventEncode03(video03); // Raise event with VideoEventArgs
            Console.WriteLine();
            
            var video04 = new Video() { Title = "Video 4" };
            var videoEncoder04 = new VideoEncoder();
            var mailServer04 = new MailServer();
            var messageService04 = new MessageServics();

            videoEncoder04.VideoEncoded04 += mailServer.onVideoEncoded;
            videoEncoder04.VideoEncoded04 += messageService.onVideoEncoded;
            videoEncoder04.EventEncode04(video04);
            Console.WriteLine();
        }
    }
}
