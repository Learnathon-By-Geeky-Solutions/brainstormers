using System;
using System.Collections.Generic;

namespace _2._Classes.Constructors
{
    internal class ConstructorDemo
    {
        public static void Learn()
        {
            var customer = new Customer();
            customer.Id = 1;
            customer.Name = "John";

            var order = new Order();
            customer.orders.Add(order);
        }
    }
}
