namespace _4._Inheritance.Constructor
{
    internal partial class ConstructorDemo
    {
        public class Vehicle
        {
            private readonly string _name;

            //public Vehicle()
            //{
            //    Console.WriteLine("Vehicle Initialized. ");
            //}

            public Vehicle(string name)
            {
                this._name = name;
            }
        }
    }
}
