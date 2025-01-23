using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Inheritance.UpcastingAndDowncasting
{
    public class DowncastingAndUpcastingDemo
    {
        public static void Learn()
        {
            // Upcasting

            Text text = new Text(); 
            Shape shape = text; // only the properties and methods of Shape class will be in the shape object.
                                // But shape and text are referencing to the same object with different view. 

            text.Width = 200;
            shape.Width = 100;

            Console.WriteLine(text.Width);  // 100

            StreamReader reader = new StreamReader(new MemoryStream()); // here parameter needs to be of Stream class,
                                                                        // but we sent MemoryStream which is a derived class of Stream class.
                                                                        // Here it is upcasted automatically. 

            var list = new ArrayList();
            list.Add(1); // needs object, so int is upcasted to object 
            list.Add("John"); // upcasted from string to object

            // Downcasting: to get all properties and methods of base and child class 
            Animal animal = new Dog(); // Upcasting
            Dog dog;

            // Safe Downcasting
            if (animal is Dog)
            {
                dog = (Dog)animal; // Downcasting
                dog.Bark(); // Accessing derived class members
            }

        }
    }
}
