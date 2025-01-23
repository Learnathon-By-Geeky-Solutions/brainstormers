using CSharpAdvanced.EventsWithParameters;

namespace CSharpAdvanced.Events;

public class VideoEncoder  // publisher 
{
    // Steps: 
    // 1- Define a delegate
    // 2- Define an event based on that delegate
    // 3- Raise the event


    //// 1- Define a delegate
    //public delegate void VideoEncodedEventHandler(object source, EventArgs args);  // the parameter can be any class derived from EventArgs class 

    //// 2- Define an event based on that delegate
    //public event VideoEncodedEventHandler VideoEncoded;

    // The previous 2 steps can be written like this
    public event EventHandler VideoEncoded;

    public void Encode(Video video)
    {
        // Encoding Logic
        Console.WriteLine("Encoding video...");
        Thread.Sleep(2000);

        // after encoding the video, we have to notify to the subscribers that the video was encoded

        //_mailService.Send(new Mail()) // this is not extensible, so we will do it another way given below

        OnVideoEncoded();
    }

    // 3- Raise the event
    protected virtual void OnVideoEncoded()
    {
        if (VideoEncoded != null) // if there is any subscriber to this event
        {
            VideoEncoded(this, EventArgs.Empty);
        }
    }

}


// Inside the publisher class, some event is going to happen. After (or during or before) happening the event, 
// the publisher wants to notify the subscriber classes. Let the notification is like "The event is happened". 
// publisher class will contain the delegate and an event based on the delegate and a method to raise the event. 
// subscriber classes will contain the method which will be invoked when the event is triggered (or raised). 