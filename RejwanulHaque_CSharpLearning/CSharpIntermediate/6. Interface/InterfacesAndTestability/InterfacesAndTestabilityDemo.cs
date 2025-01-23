using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _6._Interface.InterfacesAndTestability
{
    internal class InterfacesAndTestabilityDemo
    {
        public static void Learn()
        {
            var orderProcessor = new OrderProcessor(new ShippingCalculator());
            var order = new Order { Dateplaced = DateTime.Now, TotalPrice = 100f };
            orderProcessor.Process(order);
        }
    }
}
