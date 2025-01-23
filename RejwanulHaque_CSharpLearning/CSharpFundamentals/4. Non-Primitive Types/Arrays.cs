using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Non_Primitive_Types
{
    public class Arrays
    {
        public void IntArray()
        {
            var numbers = new int[3];
            numbers[0] = 1;

            Console.WriteLine(numbers[0]);
            Console.WriteLine(numbers[1]);
        }
        
        public void StringArray()
        {
            var strings = new string[3] { "String 1", "String 2", "String 3" };

            Console.WriteLine(strings[0]);
            Console.WriteLine(strings[1]);
            Console.WriteLine(strings[2]);
        }
    }
}
