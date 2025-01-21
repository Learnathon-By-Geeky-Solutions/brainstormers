using System;
using Fundamentals;

namespace Testability.Tests
{
    [TestClass]
    public class OrderProcessorTest
    {
        // METHODNAME_CONDITION_EXPECTATION
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Process_OrderIsShipped_ThrowsAnException()
        {
            var orderProcessor = new OrderProcessor(new FakeShippingCalc());
            var order = new OrderT{
                Shipment = new Shipment()
            };
            orderProcessor.Process(order);
        }

        [TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void Process_OrderIsNotShipped_ShouldSetTheShipmentPropertyOfTheOrder()
        {
            var orderProcessor = new OrderProcessor(new FakeShippingCalc());
            var order = new OrderT();
            orderProcessor.Process(order);
            Assert.IsTrue(order.IsShipped);
            Assert.AreEqual(1, order.Shipment.Cost);
            Assert.AreEqual(DateTime.Today.AddDays(1), order.Shipment.ShippingDate);
        }
    }

    public class FakeShippingCalc : IShippingCalc
    {
        public float Calculate(OrderT order)
        {
            return 1;
        }
    }
}

