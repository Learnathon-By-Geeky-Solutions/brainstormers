using System;
using System.Collections.Generic;

namespace _2._Classes.Methods
{
    internal class MethodsDemo
    {
        public static void Learn()
        {
            try
            {
                var point = new Point(10, 20);
                Console.WriteLine($"Point: ({point.x}, {point.y})");
                point.Move(10, -5);
                Console.WriteLine($"Point moved to ({point.x}, {point.y})");

                point.Move(null); // exception

            }
            catch (Exception)
            {
                Console.WriteLine("An unexpected error occured!");
            }


            var paramsdemo = new ParamsDemo();
            paramsdemo.Learn();

        }
    }
}
