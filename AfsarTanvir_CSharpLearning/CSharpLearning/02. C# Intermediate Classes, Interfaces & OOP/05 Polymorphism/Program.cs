// Abstract -> Indicates taht a class or a member is missing implementation
//          -> If a member is declared as abstract, the containing class needs to be declared as abstract too.
//          -> Can not be instantiated
// Sealed Modifier -> Prevents derivation of classes or overriding of methods
namespace _05_Polymorphism
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\t\t 05 Polymorphism \t\t");

            Console.WriteLine("\n-------------------\n");
            Method_Overriding.Class01.run();

            Console.WriteLine("\n-------------------\n");
            Abstract_Classes_and_Members.AbstractClasses01.run();

            Console.WriteLine("\n-------------------\n");
        }
    }
}
