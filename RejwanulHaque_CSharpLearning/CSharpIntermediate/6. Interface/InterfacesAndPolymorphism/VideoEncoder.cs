namespace _6._Interface.InterfacesAndPolymorphism;

public class VideoEncoder
{
    private readonly IList<INotificationChannel> _notificationChannels;
    public VideoEncoder()
    {
        _notificationChannels = new List<INotificationChannel>();
    }

    public void Encode()
    {
        foreach (var channel in _notificationChannels)
        {
            channel.SendNotification("This is a Message"); // at runtime, when we call the SendNotification method,
                                                                  // depending on the runtime type of that notification 
                                                                  // different notification method is going to be called.
                                                                  // That's what we call polymorphism.
        }
    }

    public void RegisterNotificationChannels(INotificationChannel channel)
    {
        _notificationChannels.Add(channel);
    }
}