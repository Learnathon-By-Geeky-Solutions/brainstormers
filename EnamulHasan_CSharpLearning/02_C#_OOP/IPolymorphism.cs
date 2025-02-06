using System;

namespace Fundamentals
{
    public class MailNotificationChannel : INotificationChannel
    {
        public void Send(Message message)
        {
            Console.WriteLine("Sending mail...");
        }
    }

    public class SmsNotificationChannel : INotificationChannel
    {
        public void Send(Message message)
        {
            Console.WriteLine("Sending sms...");
        }
    }

    public interface INotificationChannel
    {
        void Send(Message message);
    }

    public class Message
    {
        public required string Content { get; set; }
    }

    public class Video
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
    }

    public class VideoEncoder
    {
        private readonly IList<INotificationChannel> _notificationChannels;

        public VideoEncoder()
        {
            _notificationChannels = new List<INotificationChannel>();
        }

        public void Encode(Video video)
        {
            foreach (var item in _notificationChannels)
            {
                item.Send(new Message { Content = "Video encoded successfully." });
            }
        }

        public void RegisterNotificationChannel(INotificationChannel channel)
        {
            _notificationChannels.Add(channel);
        }
    }
}

