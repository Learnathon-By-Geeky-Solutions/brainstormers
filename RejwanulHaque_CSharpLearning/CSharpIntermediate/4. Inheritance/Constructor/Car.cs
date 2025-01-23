namespace _4._Inheritance.Constructor
{
    internal partial class ConstructorDemo
    {
        public class Car: Vehicle
        {
            public Car(string name)
                : base(name)
            {
                Console.WriteLine("Car Initialized. ");
            }
        }
    }
}
