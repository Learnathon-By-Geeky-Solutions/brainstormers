using _4._Inheritance.UpcastingAndDowncasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _4._Inheritance.Constructor.ConstructorDemo;

namespace _4._Inheritance.Constructor
{
    internal partial class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Constructor: ");
            ConstructorDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("DowncastingAndUpcasting: ");
            DowncastingAndUpcastingDemo.Learn();
            Console.WriteLine();
        }
    }
}
