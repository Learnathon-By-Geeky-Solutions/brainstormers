// Events -> an event is a special kind of delegate that is used to notify
// other objects when something happens. Events provide a way for an object to
// communicate changes or actions to other objects without tightly coupling them.
// Benefits : Enables loosely coupled communication, Promotes modularity,
//            Simplifies maintaining and scaling the codebase.
namespace Events
{
    public class MailServer
    {
        public void onVideoEncoded(object source, EventArgs args)
        {
            Console.WriteLine("MailServer : Sending an email...");
        }
        public void onVideoEncoded(object source, VideoEventArgs args)
        {
            Console.WriteLine("MailServer : Sending an email to " + args.Video02.Title);
        }
    }
}
