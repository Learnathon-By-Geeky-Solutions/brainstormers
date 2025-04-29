namespace TaskForge.Application
{
    public class EmailSettings
    {
        public required string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }
}
