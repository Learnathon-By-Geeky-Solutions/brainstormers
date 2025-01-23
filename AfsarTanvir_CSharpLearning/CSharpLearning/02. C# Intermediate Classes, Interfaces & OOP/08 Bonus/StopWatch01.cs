namespace _08_Bonus
{
    internal class StopWatch01
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private bool _isRunning;

        public void Start() 
        {
            if (_isRunning)
                throw new InvalidOperationException("Stopwatch is already running!");

            _startTime = DateTime.Now;
            _isRunning = true;
        }
        public void Stop() 
        {
            if (!_isRunning)
                throw new InvalidOperationException("Stopwatch is not running!");

            _endTime = DateTime.Now;
            _isRunning = false;
        }

        public TimeSpan GetInterval()
        {
            return _endTime - _startTime;
        }
        public static void run()
        {
            var stopWatch = new StopWatch01();

            for(var i=0; i<2; ++i)
            {
                stopWatch.Start();
                Thread.Sleep(1000);
                stopWatch.Stop();

                Console.WriteLine("Duration : "+stopWatch.GetInterval());
                Console.WriteLine("Press Enter to run the stopwatch one more time.");
                Console.ReadLine();
            }
        }
    }
}
