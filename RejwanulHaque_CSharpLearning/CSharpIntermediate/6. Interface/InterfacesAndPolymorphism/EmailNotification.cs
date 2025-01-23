namespace _6._Interface.InterfacesAndPolymorphism;

public class EmailNotification: INotificationChannel
{
    public void SendNotification(string message)
    {
        Console.WriteLine("Sending Email...");
    }
}