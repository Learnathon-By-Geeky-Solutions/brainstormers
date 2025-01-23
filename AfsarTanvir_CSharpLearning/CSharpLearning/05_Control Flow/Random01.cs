namespace _05_Control_Flow
{
    internal class Random01
    {
        public static void run()
        {
            var random = new Random();
            for(var i=0; i<3; ++i)
            {
                Console.WriteLine(random.Next());
            }
            for(var i=0; i<3; ++i)
            {
                Console.WriteLine(random.Next(1, 10));
            }
            Console.WriteLine((int) 'a');
            for (var i = 0; i < 10; ++i)
            {
                Console.Write( (char) random.Next(97, 122));
            }
            Console.WriteLine();

            var buffer = new char[10];
            for(var i=0; i<10; ++i)
            {
                buffer[i] = (char)('a' + random.Next(0, 26));
            }
            var password = new string(buffer);
            Console.WriteLine(password);
        }
    }
}
