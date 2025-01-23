using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6._Interface.InterfacesAndPolymorphism
{
    internal class InterfacesAndPolymorphismDemo
    {
        public static void Learn()
        {
            var encoder = new VideoEncoder();
            encoder.RegisterNotificationChannels(new EmailNotification());
            encoder.RegisterNotificationChannels(new SmsNotification());
            encoder.Encode();
        }
    }
}
