namespace _02_Classes
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Department { get; set; }
    }
    internal class ObjectInitializers01
    {

        public static void run()
        {
            // Using an object initializer
            Person person1 = new Person
            {
                Name = "John Doe",
                Age = 25,
                Department = "IT"
            };

            // Another example of an object initializer
            Person person2 = new Person
            {
                Name = "Jane Smith",
                Age = 30,
                Department = "HR"
            };

            // Displaying details
            Console.WriteLine("Person 1: Name = {0}, Age = {1}, Department = {2}", person1.Name, person1.Age, person1.Department);
            Console.WriteLine("Person 2: Name = {0}, Age = {1}, Department = {2}", person2.Name, person2.Age, person2.Department);
        }
        
    }
}
