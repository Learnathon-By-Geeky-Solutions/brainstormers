namespace _3._Association_Between_Classes.Composition
{
    public class Installer
    {
        private readonly Logger _logger;

        public Installer(Logger logger)
        {
            this._logger = logger;
        }
        public void Install()
        {
            Console.WriteLine("We are installing...");
        }
    }
}
