using System;

namespace Fundamentals
{

    public class FileLogger : ILogger
    {
        private readonly string _path;

        public FileLogger(string path)
        {
            _path = path;
        }

        public void LogInfo(string message)
        {
            using(var streamWriter = new StreamWriter(_path, true))
            {
                streamWriter.WriteLine(message);
            }
        }
    }

    public class ConsoleLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
        }
    }

    public interface ILogger
    {
        void LogInfo(string message);
    }

    public class DbMigrator
    {
        private readonly ILogger _logger;

        public DbMigrator(ILogger logger){
            _logger = logger;
        }

        public void Migrate()
        {
            _logger.LogInfo("Migration started at " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            _logger.LogInfo("Migration finished at " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
        }
    }
}

