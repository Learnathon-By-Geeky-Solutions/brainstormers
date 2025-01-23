namespace _04_Non_Primitive_Types
{
    public class Person_
    {
        public int Age;
    }
    internal class ReferenceTypesAndValueTypes01
    {
        public static void run()
        {
            var a = 10;
            var b = a;
            ++b;
            // here it's pass by value
            Console.WriteLine(string.Format("a: {0}, b: {1}", a, b));

            var array01 = new int[] { 1, 2, 3 };
            var array02 = array01;
            array02[0] = 1;
            //here it's pass by reference, so it change both place
            Console.WriteLine(string.Format("array01[0]: {0}, array02[0]: {1}", array01[0], array02[0]));

            var number = 1;
            Console.WriteLine("Before calling method number: "+ number);
            Increment(number); 
            Console.WriteLine("After calling method number: "+ number);

            var person = new Person_() { Age = 20 };
            Console.WriteLine("Before calling method Age: "+ person.Age);
            MakeOld(person);
            Console.WriteLine("After calling method Age: "+ person.Age);
        }
        public static void Increment(int number) { number += 10; }
        public static void MakeOld(Person_ a)
        {
            a.Age += 10;
        }
    }
}
