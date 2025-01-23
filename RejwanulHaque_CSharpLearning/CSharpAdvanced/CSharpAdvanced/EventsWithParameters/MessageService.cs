namespace CSharpAdvanced.EventsWithParameters;

public class MessageService
{
    public void OnVideoEncoded(object source, VideoEventArgs e)
    {
        Console.WriteLine("MessageService: Sending a Message..."+ e.Video.Title);
    }
}