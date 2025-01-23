namespace _02_Classes
{
    public class Order
    {

    }
    public class Custormer01
    {
        public int id;
        public string Name;
        public List<Order> Orders;

        // constructors
        public Custormer01()
        {
            var orders = new List<Order>();
        }
        public Custormer01(int id) : this()
        {
            this.id = id;
        }
        public Custormer01(int id, string name) : this(id)
        {
            this.Name = name;
        }

        public void Details()
        {
            Console.WriteLine("Name : {0}, ID : {1}", Name, id);
        }
    }
    internal class Constructors01
    {
        public static void run()
        {
            Console.WriteLine("Customer 1 Details");
            var customer01 = new Custormer01();
            customer01.Details();
            customer01.id = 101;
            customer01.Name = "Rajon";
            customer01.Details();

            Console.WriteLine("Customer 2 Details");
            var customer02 = new Custormer01(102);
            customer02.Details();
            customer02.Name = "Enamul";
            customer02.Details();

            Console.WriteLine("Customer 3 Details");
            var customer03 = new Custormer01(103, "Afsar");
            customer03.Details();

        }
    }
}
