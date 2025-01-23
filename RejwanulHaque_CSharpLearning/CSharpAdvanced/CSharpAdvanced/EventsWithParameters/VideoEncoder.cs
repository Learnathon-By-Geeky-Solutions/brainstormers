namespace CSharpAdvanced.EventsWithParameters;

public class VideoEncoder  // publisher 
{
    // Steps: 
    // 1- Define a delegate
    // 2- Define an event based on that delegate
    // 3- Raise the event

    //public delegate void VideoEncodedEventHandler(object source, VideoEventArgs args);  
    //public event VideoEncodedEventHandler VideoEncoded;

    // The previous 2 lines can be written like this
    public event EventHandler<VideoEventArgs> VideoEncoded;
    public void Encode(Video video)
    {

        Console.WriteLine("Encoding video...");
        Thread.Sleep(2000);

        OnVideoEncoded(video);
    }

    protected virtual void OnVideoEncoded(Video video)
    {
        if (VideoEncoded != null)
        {
            VideoEncoded(this, new VideoEventArgs(){Video = video});
        }
    }
}
