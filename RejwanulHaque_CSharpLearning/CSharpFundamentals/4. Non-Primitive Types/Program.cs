using System;
using _4._Non_Primitive_Types.Math;

namespace _4._Non_Primitive_Types
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Person Class: ");
            Person john = new Person();
            john.name = "John";
            john.Introduce();

            Calculator calculator = new Calculator();
            Console.WriteLine(calculator.Add(5, 4));

            Console.WriteLine();
            Console.WriteLine("Arrays Class: ");


            var a = new Arrays();
            a.IntArray();
            a.StringArray();

            Strings john_smith = new Strings
            {
                firstName = "John",
                lastName = "Smith"
            };
            john_smith.Introduce();

            john_smith.StringFromArray();
            john_smith.PrintVerbatimString();

            Console.WriteLine();
            Console.WriteLine("Enums Class: ");

            Enums method1 = new Enums();
            method1.ShippingMethodCheck();
            method1.ShowDayName();
            method1.PrintMonths();
            method1.MakeShippingMethod("Express");  // Express has to be inside the Enum ShippingMethod
            method1.MakeDay("Friday");  // Friday has to be inside the Enum Day

            Console.WriteLine();
            Console.WriteLine("ReferenceTypesAndValueTypes Class: ");

            ReferenceTypesAndValueTypes referenceTypesAndValueTypes = new ReferenceTypesAndValueTypes();
            referenceTypesAndValueTypes.ValueType();
            referenceTypesAndValueTypes.ReferenceType();

            Console.WriteLine();
            Console.WriteLine("PassByValueAndPassByReference:");


            PassByValueAndPassByReference passByValueAndPassByReference = new PassByValueAndPassByReference();
            var val1 = 1;
            passByValueAndPassByReference.Increment(val1);
            Console.WriteLine($"Value is {val1} in main method");
            john.Age = 25;
            passByValueAndPassByReference.MakeOlder(john);
            Console.WriteLine($"Age of {john.name} is {john.Age} in method");
        }
    }
}