namespace _03_Association_Between_Classes.Composition
{
    public class Installer01
    {
        private readonly Logger01 _logger;
        public Installer01(Logger01 logger)
        {
            _logger = logger;
        }
        public void Install()
        {
            _logger.log("We are installing the application...");
        }
    }
}
