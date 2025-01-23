using _3._Association_Between_Classes.Composition;
using _3._Association_Between_Classes.Inheritance;
using System;
using System.Collections.Generic;
using static _3._Association_Between_Classes.Composition.Logger;


namespace _3._Association_Between_Classes
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Inheritance: ");
            InheritanceDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Composition: ");
            CompositionDemo.Learn();
            Console.WriteLine();

        }
    }
}
