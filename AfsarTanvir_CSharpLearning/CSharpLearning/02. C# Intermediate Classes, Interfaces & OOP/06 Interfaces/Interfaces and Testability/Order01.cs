namespace _06_Interfaces.Interfaces_and_Testability
{
    public class Order01
    {
        public int Id { get; set; }
        public DateTime DatePlaced { get; set; }
        public Shipment01? Shipment { get; set; }
        public float TotalPrice { get; set; }
        public bool IsShipped
        {
            get { return Shipment != null; }
        }
    }
}
