using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Non_Primitive_Types
{
    public class ReferenceTypesAndValueTypes
    {
        /*
 
         * Primitive types and Custom Structures are Structures. Allocated automatically (on stack) and removed when out of scope
         * Arrays, Strings and Custom Classes are Classes. Allocated manually (on heap). Garbage collected by CLR which is of no use. 

         */

        public void ValueType()
        {
            var a = 10;
            var b = a; // b stores the value of a, not the address of a 
            b++;
            Console.WriteLine($"a: {a}, b: {b}");
        }
        public void ReferenceType()
        {
            var array1 = new int[3] { 1, 2, 3 }; // array1 stores (in stack) the address of heap where {1, 2, 3} are stored 
            var array2 = array1; // array2 stores (in stack) the address of array1
            array2[0] = 0;
            Console.WriteLine(array1[0]);
        }
    }
}
