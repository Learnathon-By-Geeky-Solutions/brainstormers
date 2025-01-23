namespace _06_Interfaces.Interfaces_and_Testability
{
    internal class Class01
    {
        public static void run()
        {
            Console.WriteLine("\t Interfaces and Testability\n");
            var orderProcessor = new OrderProcessor01(new ShippingCalculator01());
            var order01 = new Order01 { DatePlaced = DateTime.Now, TotalPrice = 100f };
            var order02 = new Order01 { DatePlaced = DateTime.Now, TotalPrice = 15f };

            orderProcessor.Process(order01);

            Console.WriteLine();
            orderProcessor.Process(order02);
        }
    }
}
