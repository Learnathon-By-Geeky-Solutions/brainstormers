// Events -> an event is a special kind of delegate that is used to notify
// other objects when something happens. Events provide a way for an object to
// communicate changes or actions to other objects without tightly coupling them.
// Benefits : Enables loosely coupled communication, Promotes modularity,
//            Simplifies maintaining and scaling the codebase.
namespace Events
{
    public class VideoEncoder
    {
        // Declaring and using custom delegates for events.
        public void Encode(Video video)
        {
            Console.WriteLine("Encoding Video... (1)");
            Thread.Sleep(1000); // end after 1 second or 1000 milesecond
        }

        // 1-> Define a delegate
        // 2-> Define an event based on that delegate
        // 3-> Rasie the event

        // Using EventArgs and custom EventArgs for passing data.
        public delegate void VideoEncodeEventHandler(object source, EventArgs args); // Generic event
        public event VideoEncodeEventHandler VideoEncoded; // Event declaration
        public void EventEncode(Video video)
        {
            Console.WriteLine("Event Encoding Video... (2)");
            Thread.Sleep(1000); // Simulate encoding

            OnVideoEncoded(); // Trigger event
        }
        protected virtual void OnVideoEncoded()
        {
            VideoEncoded?.Invoke(this, EventArgs.Empty); // Safely invoke event
        }

        public delegate void VideoEventArgsHandler03(object source, VideoEventArgs args);
        public event VideoEventArgsHandler03 VideoEncoded03;
        public void EventEncode03(Video video)
        {
            Console.WriteLine("Event Args Encoding Video... (3)");
            Thread.Sleep(1000);

            OnVideoEncoded03(video);
        }
        protected virtual void OnVideoEncoded03(Video video)
        {
            if (VideoEncoded03 != null)
                VideoEncoded03(this, new VideoEventArgs() { Video02 = video});
        }

        // EventHandler<TEventArgs>
        // EventHandler
        // Using the standard EventHandler and EventHandler<TEventArgs>.
        public event EventHandler<VideoEventArgs> VideoEncoded04;
        // public event EventHandler VideoEncoded04; // if you want to publish an event without any additional data
        public void EventEncode04(Video video)
        {
            Console.WriteLine("Event Args Encoding Video... (4)");
            Thread.Sleep(1000);

            OnVideoEncoded04(video);
        }
        protected virtual void OnVideoEncoded04(Video video)
        {
            if (VideoEncoded04 != null)
                VideoEncoded04(this, new VideoEventArgs() { Video02 = video });
        }
    }
}
