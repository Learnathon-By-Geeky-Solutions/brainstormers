// The dynamic keyword in C# allows you to bypass compile-time type checking and work with types whose methods or properties are resolved at runtime
namespace Dynamic
{
    internal class Program
    {
        public static void run1()
        {
            // Example 1: Using dynamic for different types
            dynamic dynamicVar = 10; // Holds an integer
            Console.WriteLine("Value: " + dynamicVar + ", Type: " + dynamicVar.GetType());

            dynamicVar = "Hello, Dynamic!"; // Holds a string
            Console.WriteLine("Value: " + dynamicVar + ", Type: " + dynamicVar.GetType());

            dynamicVar = DateTime.Now; // Holds a DateTime object
            Console.WriteLine("Value: " + dynamicVar + ", Type: " + dynamicVar.GetType());

            // Example 2: Calling methods dynamically
            dynamic obj = new ExampleClass01();
            obj.PrintMessage("Dynamic Method Call!");

            // Example 3: Safely checking the type before accessing properties
            dynamicVar = "Dynamic String";
            if (dynamicVar is string)
            {
                int length = dynamicVar.Length; // Safe access to the Length property
                Console.WriteLine("String Length: " + length);
            }
            else
            {
                Console.WriteLine("DynamicVar is not a string, skipping Length access.");
            }

            // Example 4: Runtime error prevention
            try
            {
                // Attempt to access an invalid property (commented out to prevent crash)
                // obj.NonExistentMethod();
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                Console.WriteLine("Runtime Error: " + ex.Message);
            }

            // Example 5: Dynamic with collections
            dynamic[] dynamicArray = { 1, "two", 3.0, true };
            foreach (dynamic item in dynamicArray)
            {
                Console.WriteLine("Value: " + item + ", Type: " + item.GetType());
            }
        }
        public static void run2()
        {
            // Example 1: Casting dynamic to int
            dynamic dynamicVar = 42; // Initially holds an integer
            Console.WriteLine("Original dynamic value: " + dynamicVar);

            int intValue = (int)dynamicVar; // Explicit cast to int
            Console.WriteLine("Casted to int: " + intValue);

            // Example 2: Casting dynamic to string
            dynamicVar = "Hello, Dynamic!";
            Console.WriteLine("\nOriginal dynamic value: " + dynamicVar);

            string stringValue = (string)dynamicVar; // Explicit cast to string
            Console.WriteLine("Casted to string: " + stringValue);

            // Example 3: Casting dynamic to DateTime
            dynamicVar = DateTime.Now;
            Console.WriteLine("\nOriginal dynamic value: " + dynamicVar);

            DateTime dateValue = (DateTime)dynamicVar; // Explicit cast to DateTime
            Console.WriteLine("Casted to DateTime: " + dateValue);

            // Example 4: Attempting invalid casting
            try
            {
                dynamicVar = "This is a string";
                int invalidCast = (int)dynamicVar; // Invalid cast: Will throw exception
                Console.WriteLine("Casted to int: " + invalidCast);
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("\nInvalidCastException caught: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nException caught: " + ex.Message);
            }

            // Example 5: Casting between custom classes
            dynamic dynamicObj = new ExampleClass02 { Value = 100 };
            ExampleClass02 obj = (ExampleClass02)dynamicObj; // Explicit cast to ExampleClass01
            Console.WriteLine("\nCasted to ExampleClass01: Value = " + obj.Value);
        }
        public static void run3()
        {
            dynamic a = 5; // Dynamic variable initialized with an integer
            int b = 10;    // Static variable of type int
            var result1 = a + b; // Dynamic and int combination
            Console.WriteLine($"Result1 (a + b): {result1}, Type: {result1.GetType()}");

            a = 3.5; // Change dynamic variable to a double
            var result2 = a + b; // Dynamic (double) and int combination
            Console.WriteLine($"Result2 (a + b): {result2}, Type: {result2.GetType()}");

            a = "Hello";
            string c = " World!";
            var result3 = a + c; // Dynamic (string) and string combination
            Console.WriteLine($"Result3 (a + c): {result3}, Type: {result3.GetType()}");

            a = 2.5;
            float d = 1.5f;
            var result4 = a + d; // Dynamic (double) and float combination
            Console.WriteLine($"Result4 (a + d): {result4}, Type: {result4.GetType()}");

            a = new { Name = "Dynamic", Value = 100 };
            Console.WriteLine($"Result5 (Anonymous): Name = {a.Name}, Value = {a.Value}, Type: {a.GetType()}");

            // Handling potential errors
            try
            {
                int invalidOperation = a + b; // Throws a RuntimeBinderException
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                Console.WriteLine("RuntimeBinderException: " + ex.Message);
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("\n--------------- Running 1 : ");
            run1();

            Console.WriteLine("\n--------------- Running 2 : (Type Casting) ");
            run2();

            Console.WriteLine("\n--------------- Running 3 : (Mixing and Static Types) ");
            run3();
        }
    }
    class ExampleClass02
    {
        public int Value { get; set; }
    }
    class ExampleClass01
    {
        public void PrintMessage(string message)
        {
            Console.WriteLine("Message: " + message);
        }
    }
}
