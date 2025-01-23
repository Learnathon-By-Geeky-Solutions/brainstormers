namespace _03_Association_Between_Classes.Composition
{
    internal class composition01
    {
        public static void run()
        {
            var dbMigrate = new DbMigrator(new Logger01());

            var logger = new Logger01();
            var installer = new Installer01(logger);

            dbMigrate.Migrate();
            installer.Install();
        }
    }
}
