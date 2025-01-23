namespace _2._Classes.Constructors
{
    public class Customer
    {
        public int Id;
        public string Name;
        public readonly List<Order> orders; // defined 'readonly' so that it is not changed inside customer class

        public Customer()
        {
            orders = new List<Order>();
        }
        public Customer(int id)
            :this() // first call Customer() constructor 
        {
            this.Id = id;
        }

        public Customer(int id, string name)
            :this(id)  // first call Customer(id) constructor 
        {
            this.Name = name;
        }
    }
}
