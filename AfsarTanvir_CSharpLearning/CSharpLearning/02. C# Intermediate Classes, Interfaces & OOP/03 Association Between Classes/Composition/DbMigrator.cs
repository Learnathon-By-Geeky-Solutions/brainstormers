namespace _03_Association_Between_Classes.Composition
{
    public class DbMigrator
    {
        private readonly Logger01 _logger;
        public DbMigrator(Logger01 logger)
        {
            _logger = logger;
        }
        public void Migrate()
        {
            _logger.log("We are migrating blah blah blah...");
        }
    }
}
