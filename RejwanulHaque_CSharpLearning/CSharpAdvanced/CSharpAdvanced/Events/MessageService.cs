namespace CSharpAdvanced.Events;

public class MessageService
{
    public void OnVideoEncoded(object source, EventArgs e)
    {
        Console.WriteLine("MessageService: Sending a Message...");
    }
}