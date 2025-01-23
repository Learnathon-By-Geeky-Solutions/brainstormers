using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5._Polymorphism.AbstractClassesAndMembers
{
    internal class AbstractClassesAndMembersDemo
    {
        public static void Learn()
        {
            // var shape = new Shape(); // error: Shape cannot be instantiated

            var circle = new Circle();
            circle.Draw();

            var rectangle = new Rectangle();
            rectangle.Draw();
        }
    }
}

/*
 * 
 * Abstract classes:
 * Do not include implementation 
 * If a member is declared as abstract, the containing class needs to be declared abstract too
 * Derived classes must implement all abstract members (of the base abstract class)
 * Cannot be instantiated
 *
 */
