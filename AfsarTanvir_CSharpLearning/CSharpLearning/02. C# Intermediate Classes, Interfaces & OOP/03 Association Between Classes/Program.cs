// Coupling -> A measure of how interconnected classes and subsystems are
// Inheritance -> A knd of relationship between two classes that allows one to inherit code from the other (is-A relationship, re-use)
// Composition -> A kind of relationship between two classes that allows one to contain the other (Has-a relationship, re-use)
using _03_Association_Between_Classes.Composition;
using _03_Association_Between_Classes.Inheritance;

namespace _03_Association_Between_Classes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n-------------------\n");
            Inheritance01.run();          
            
            Console.WriteLine("\n-------------------\n");

            composition01.run();
            Console.WriteLine("\n-------------------\n");
        }
    }
}
