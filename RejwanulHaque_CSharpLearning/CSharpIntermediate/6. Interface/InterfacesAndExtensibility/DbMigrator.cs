namespace _6._Interface.InterfacesAndExtensibility;

public class DbMigrator
{
    private readonly ILogger _logger;

    public DbMigrator(ILogger logger)
    {
        _logger = logger;
    }
    public void Migrate()
    {
        _logger.LogInfo($"Migrating started at {DateTime.Now}");
        _logger.LogInfo($"Migrating finished at {DateTime.Now}");

        //Console.WriteLine($"Migrating started at {DateTime.Now}");
        //Console.WriteLine($"Migrating finished at {DateTime.Now}");
    }
}