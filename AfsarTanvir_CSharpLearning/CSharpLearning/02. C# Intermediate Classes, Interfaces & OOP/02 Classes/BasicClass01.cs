namespace _02_Classes
{
    public class Person01
    {
        public string Name;
        public int Age;
        public string Department;
        // instance method
        public void Introduce(string to)
        {
            Console.WriteLine($"Hi {to}, I am {this.Name}");
        }
        // static method
        public static Person01 Parse(string s, int age, string dept)
        {
            var person01 = new Person01();
            person01.Name = s;
            person01.Age = age;
            person01.Department = dept;

            return person01;
        }

        // The prefix Static makes it clear that this method belongs to the class and not an object.
        public static void DisplayPersonDetails_Static(Person01 person)
        {
            Console.WriteLine("Name : {0}, Age : {1}, Dept : {2}", person.Name, person.Age, person.Department);
        }

        // The prefix Instance indicates that this method is called on a specific instance of the class.
        public void DisplayPersonDetails_Instance()
        {
            Console.WriteLine("Name : {0}, Age : {1}, Dept : {2}", Name, Age, Department);
        }

    }
    internal class BasicClass01
    {
        public static void run()
        {
            Person01 person01 = new Person01();
            person01.Name = "Afsar Tanvir";
            person01.Age = 24;
            person01.Department = "CSE";

            person01.Introduce("Rajon and Enamul");
            var person02 = Person01.Parse("Bodrul", 22, "CSE");

            Person01.DisplayPersonDetails_Static(person01);
            person02.DisplayPersonDetails_Instance();
        }
    }
}
