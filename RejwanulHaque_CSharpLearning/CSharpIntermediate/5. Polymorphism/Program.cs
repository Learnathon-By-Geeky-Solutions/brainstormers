using _5._Polymorphism.AbstractClassesAndMembers;
using _5._Polymorphism.MethodOverriding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5._Polymorphism
{
    internal partial class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("MethodOverriding: ");
            MethodOverridingDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("AbstractClassesAndMembers: ");
            AbstractClassesAndMembersDemo.Learn();
            Console.WriteLine();
        }
    }
}