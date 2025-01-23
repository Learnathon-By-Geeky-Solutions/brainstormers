namespace _04_Inheritance.Constructor_and_Inheritance
{
    public class Vehicle
    {
        private readonly string _registrationNumber;

        public Vehicle()
        {
            Console.WriteLine("Vehicle is begin initialized.");
        }

        public Vehicle(string registrationNumber)
        {
            this._registrationNumber = registrationNumber;
            Console.WriteLine("Registration number is : " + registrationNumber);
        }
    }
}
