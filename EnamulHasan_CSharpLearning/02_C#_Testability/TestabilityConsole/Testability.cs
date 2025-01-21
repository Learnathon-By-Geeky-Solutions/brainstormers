using System;

namespace Fundamentals
{
    public class OrderT
    {
        public DateTime DatePlaced { get; set; }
        public bool IsShipped { 
            get { return Shipment != null; }
            set { }
        }
        public float TotalPrice { get; set; }
        public Shipment Shipment { get; set; }
    }

    public class Shipment
    {
        public DateTime ShippingDate { get; set; }
        public float Cost { get; set; }
    }

    public interface IShippingCalc
    {
        float Calculate(OrderT order);

    }

    public class ShippingCalc : IShippingCalc
    {
        public float Calculate(OrderT order)
        {
            if (order.TotalPrice < 30f)
                return order.TotalPrice * 0.1f;

            return 0;
        }
    }

    public class OrderProcessor
    {
        private readonly IShippingCalc _shippingCalc;

        public OrderProcessor(IShippingCalc shippingCalc)
        {
            _shippingCalc = shippingCalc;
        }

        public void Process(OrderT order)
        {
            if(order.IsShipped)
                throw new InvalidOperationException("This order is already processed.");
        
            order.Shipment = new Shipment
            {
                Cost = _shippingCalc.Calculate(order),
                ShippingDate = DateTime.Today.AddDays(1)
            };
        }
    }

}

