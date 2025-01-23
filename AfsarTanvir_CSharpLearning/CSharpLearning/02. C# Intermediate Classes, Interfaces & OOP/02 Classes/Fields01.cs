namespace _02_Classes
{
    public class Custormer02
    {
        public int Id;
        public string Name;
        public List<Order02> Orders = new List<Order02>();

        public Custormer02()
        {

        }
        public Custormer02(int id)
        {
            this.Id = id;
        }
        public Custormer02(int id, string name) : this(id)
        {
            this.Name = name;
        }
        public void Promote()
        {
            Orders = new List<Order02>();
        }
    }
    public class Order02
    {

    }
    internal class Fields01
    {
        public static void run()
        {
            var customer = new Custormer02(1);
            customer.Orders.Add(new Order02());
            customer.Orders.Add(new Order02());

            //customer.Promote(); // if we comment it the result will be 2, if we don't 0
            // All previously added Order02 objects in the Orders list are removed because the reference to the original list is replaced.

            Console.WriteLine(customer.Orders.Count);
        }
    }
}
