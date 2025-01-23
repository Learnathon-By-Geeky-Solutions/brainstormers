namespace _6._Interface.InterfacesAndPolymorphism;

public class SmsNotification: INotificationChannel
{
    public void SendNotification(string message)
    {
        Console.WriteLine("Sending SMS...");
    }
}