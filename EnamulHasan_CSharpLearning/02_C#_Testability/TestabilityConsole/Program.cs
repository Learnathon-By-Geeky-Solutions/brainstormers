﻿using System;
using Fundamentals;

class Program
{
    static void Main(string[] args)
    {
        //Unit Tests
        var orderProcessor = new OrderProcessor(new ShippingCalc());
        var orderT = new OrderT
        {
            DatePlaced = DateTime.Now,
            TotalPrice = 100f,
            Shipment = new Shipment() // Assuming Shipment has a parameterless constructor
        };
        orderProcessor.Process(orderT);

    }
}
