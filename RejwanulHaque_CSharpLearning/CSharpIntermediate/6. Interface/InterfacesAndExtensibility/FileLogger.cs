namespace _6._Interface.InterfacesAndExtensibility;

public class FileLogger : ILogger
{
    private readonly string _path;

    public FileLogger(string path)
    {
        _path = path;
    }
    public void LogError(string message)
    {
        Log(message, "Error");
    }
    public void LogInfo(string message)
    {
            Log(message, "Info");
    }

    public void Log(string message, string format)
    {

        using (var streamWriter = new StreamWriter(_path, true))
        {
            streamWriter.WriteLine(format + ": " + message);
        }
    }
}