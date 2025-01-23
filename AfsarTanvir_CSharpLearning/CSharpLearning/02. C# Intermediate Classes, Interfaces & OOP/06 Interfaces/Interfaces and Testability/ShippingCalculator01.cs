namespace _06_Interfaces.Interfaces_and_Testability
{
    public interface IShippingCalculator01
    {
        float CalculateShipping(Order01 order);
    }
    public class ShippingCalculator01 : IShippingCalculator01
    {
        public float CalculateShipping(Order01 order)
        {
            if (order.TotalPrice < 30f)
                return order.TotalPrice * 0.1f;

            return 0;
        }
    }
}
