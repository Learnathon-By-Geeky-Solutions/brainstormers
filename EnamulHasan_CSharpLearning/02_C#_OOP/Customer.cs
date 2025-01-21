using System;

namespace Fundamentals
{
    public class Order
    {

    }

    public class Customer
    {
        public Customer(){
            Orders = new List<Order>();
        }

        public Customer(string Name)
            : this()
        {
            this.Name = Name; 
        }

        public int Id;
        public string Name;
        public List<Order> Orders;

        public void Intro(){
            Console.WriteLine("Name: " + Name);
        }
    }

}

