// Object -> An instance of a class
// instance -> accessible from an object
// static -> accessible from the class
// Constructor -> To put an object in an early state
// Object Initializers -> A syntax for quickly initialising an object without the need to call one of ifs constructors
//                     -> To avoid multiple constructor
// Access Modifiers -> A way to control access to a class and/ or its memebers (Public, Private, Protected, Internal, Protected Internal)
// Properties : A class member that encapsulates a getter/ setter for accessing a field
// Indexers -> A way to access elements in a class that represents a list of values
using _02_Classes.Methods;

namespace _02_Classes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BasicClass01.run();
            Console.WriteLine("-------------------\n");

            Constructors01.run();
            Console.WriteLine("-------------------");

            ObjectInitializers01.run();
            Console.WriteLine("-------------------");

            Methods01.run();
            Methods01.MethodsOverloadingWithNullChecking();
            Console.WriteLine("-------------------");

            Fields01.run();
            Console.WriteLine("-------------------");

            AccessModifiers01.run();
            Console.WriteLine("-------------------");

            Properties01.run();
            Console.WriteLine("-------------------");

            Indexers01.run();
            Console.WriteLine("-------------------");
        }
    }
}
