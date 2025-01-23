namespace _3._Association_Between_Classes.Composition
{
    public class DBMigrator
    {
        private readonly Logger _logger;

        public DBMigrator(Logger logger)
        {
            this._logger = logger;
        }
        public void Migrate()
        {
            Console.WriteLine("We are migrating...");
        }
    }
}
