using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Non_Primitive_Types
{
    public class PassByValueAndPassByReference
    {
        public void Increment(int value) 
        {
            value += 10;
            Console.WriteLine($"Value inside Increment Method is: {value}");
        }
        public void MakeOlder(Person person)
        {
            person.Age += 10;
            Console.WriteLine($"Age of {person.name} is {person.Age} inside MakeOlder method");
        }
    }
}
