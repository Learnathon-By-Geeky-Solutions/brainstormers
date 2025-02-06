using System;

namespace Fundamentals
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Customer
    {
        public Customer()
        {
            Orders = new List<Order>();
            Name = string.Empty; // Initialize Name to avoid CS8618
        }

        public Customer(string Name)
            : this()
        {
            this.Name = Name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }

        public void Intro()
        {
            Console.WriteLine("Name: " + Name);
        }
    }

}

