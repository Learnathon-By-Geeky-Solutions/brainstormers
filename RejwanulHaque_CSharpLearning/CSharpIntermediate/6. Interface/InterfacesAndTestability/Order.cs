namespace _6._Interface.InterfacesAndTestability;

public class Order
{
    public int Id { get; set; }
    public DateTime Dateplaced { get; set; }

    public Shipment Shipment { get; set; }
    public float TotalPrice { get; set; }

    public bool IsShipped
    {
        get { return Shipment != null; }
    }

}