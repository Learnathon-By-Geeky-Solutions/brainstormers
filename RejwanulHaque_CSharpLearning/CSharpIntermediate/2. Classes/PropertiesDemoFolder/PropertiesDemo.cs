using System;
using System.Collections.Generic;


namespace _2._Classes.PropertiesDemoFolder
{
    public class PropertiesDemo
    {
        public static void Learn()
        {
            var person = new Person(new DateTime(1999, 8, 20));
            Console.WriteLine($"Age: {person.Age}");
        }
    }
}
