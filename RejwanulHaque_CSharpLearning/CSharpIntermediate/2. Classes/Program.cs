using System;
using System.Collections.Generic;
using _2._Classes.Constructors;
using _2._Classes.Indexer;
using _2._Classes.Methods;
using _2._Classes.PropertiesDemoFolder;


namespace _2._Classes
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Constructor: ");
            ConstructorDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Methods: ");
            MethodsDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Properties: ");
            PropertiesDemo.Learn();
            Console.WriteLine();

            Console.WriteLine("Indexer: ");
            IndexerDemo.Learn();
            Console.WriteLine();
        }
    }
}
