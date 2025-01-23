namespace _06_Interfaces.Interfaces_and_Testability
{
    public class OrderProcessor01
    {
        private readonly IShippingCalculator01 _shippingCalculator;

        public OrderProcessor01(IShippingCalculator01 shippingCalculator01)
        {
            _shippingCalculator = shippingCalculator01;
        }

        public void Process(Order01 order)
        {
            if (order.IsShipped)
                throw new InvalidOperationException("This order is already processed.");

            order.Shipment = new Shipment01
            {
                Cost = _shippingCalculator.CalculateShipping(order),
                ShippingDate = DateTime.Today.AddDays(1)
            };

            Console.WriteLine("Order01 processed successfully!");
            Console.WriteLine($"Shipment01 Cost: {order.Shipment.Cost}");
            Console.WriteLine($"Shipping Date: {order.Shipment.ShippingDate}");
        }
    }
}
