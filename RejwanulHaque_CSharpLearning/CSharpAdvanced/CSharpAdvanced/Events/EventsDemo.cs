using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAdvanced.Events
{
    internal class EventsDemo
    {
        public static void Learn()
        {
            var video = new Video{Title = "Video 1"};
            var videoEncoder = new VideoEncoder();  // publisher 

            var mailService = new MailService();  // subscriber
            var messageService = new MessageService();  // subscriber

            videoEncoder.VideoEncoded += mailService.OnVideoEncoded;  // subscribe to the event 
            videoEncoder.VideoEncoded += messageService.OnVideoEncoded;  // subscribe to the event 

            videoEncoder.Encode(video);
        }
    }
}
