namespace _04_Inheritance.Constructor_and_Inheritance
{
    public class Car01 : Vehicle
    {
        public Car01()
        {
            Console.WriteLine("Car is begin initialized.");
        }
        public Car01(string registrationNumber) : base(registrationNumber)
        {
            Console.WriteLine("Car is begin initialized by " + registrationNumber);
        }
    }
}
