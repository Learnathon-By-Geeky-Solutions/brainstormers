using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Inheritance
{
    internal class BoxingAndUnboxingDemo
    {
        public static void Learn()
        {
            object boxed = 42;           // Boxing
            int unboxed = (int)boxed;    // Unboxing
            Console.WriteLine($"Unboxed value: {unboxed}");



        }
    }
}


/*
 *
 * boxing and unboxing refer to the process of converting a value type (e.g., int, float)
 * to a reference type (object), and vice versa
 * 
 * try to avoid unnecessary boxing/unboxing for performance-critical applications.
 * Use generics wherever possible.
 *
 * Real-Life Analogy
 * Boxing: Think of boxing as packing a small gift (value type) into a box (reference type). You can't directly interact with the gift once it's in the box; you interact with the box instead.
 * Unboxing: Taking the gift out of the box to use it directly.
 *
 */