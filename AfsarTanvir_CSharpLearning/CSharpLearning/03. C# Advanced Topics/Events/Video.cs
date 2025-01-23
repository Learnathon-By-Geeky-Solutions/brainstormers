// Events -> an event is a special kind of delegate that is used to notify
// other objects when something happens. Events provide a way for an object to
// communicate changes or actions to other objects without tightly coupling them.
// Benefits : Enables loosely coupled communication, Promotes modularity,
//            Simplifies maintaining and scaling the codebase.
namespace Events
{
    public class Video
    {
        public required string Title { get; set; }
    }
}
