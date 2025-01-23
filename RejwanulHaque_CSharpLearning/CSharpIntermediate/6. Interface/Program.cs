using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _6._Interface.InterfacesAndExtensibility;
using _6._Interface.InterfacesAndPolymorphism;
using _6._Interface.InterfacesAndTestability;

namespace _6._Interface
{
    internal partial class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("InterfacesAndTestability: ");
            InterfacesAndTestabilityDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("InterfacesAndExtensibility: ");
            InterfacesAndExtensibilityDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("InterfacesAndPolymorphism: ");
            InterfacesAndPolymorphismDemo.Learn();
            Console.WriteLine();
        }
    }
}