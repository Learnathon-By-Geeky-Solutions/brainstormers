namespace NullableTypes
{
    internal class Program
    {
        public static void Basic01()
        {
            // DateTime is a non-nullable value type, so assigning null directly is not allowed.
            // DateTime DateTime01 = null; // Uncommenting this line will show a compile-time error.

            // Declare a nullable DateTime.
            DateTime? dateTime02 = null;

            // GetValueOrDefault() returns the default value for DateTime (01/01/0001 00:00:00) if null.
            Console.WriteLine("GetValueOrDefault(): " + dateTime02.GetValueOrDefault());

            // HasValue is a property, not a method, so no parentheses are needed.
            Console.WriteLine("HasValue: " + dateTime02.HasValue);

            // Accessing Value when dateTime02 is null will throw an exception.
            // Console.WriteLine("Value: " + dateTime02.Value); // Uncommenting this will cause a runtime error.

            // Safely accessing the value using HasValue.
            if (dateTime02.HasValue)
            {
                Console.WriteLine("Value: " + dateTime02.Value);
            }
            else
            {
                Console.WriteLine("Value: dateTime02 is null");
            }
            Console.WriteLine();


            // Assign today's date to a nullable DateTime.
            DateTime? dateTime03 = DateTime.Today;

            // GetValueOrDefault() will return the actual value since it's not null.
            Console.WriteLine("GetValueOrDefault(): " + dateTime03.GetValueOrDefault());

            // Check if the nullable DateTime has a value.
            Console.WriteLine("HasValue: " + dateTime03.HasValue);

            // Access the Value safely.
            if (dateTime03.HasValue)
            {
                Console.WriteLine("Value: " + dateTime03.Value);
            }
        }
        public static void Basic02()
        {
            DateTime? dateTime04 = new DateTime(2014, 1, 1);
            DateTime dateTime05 = dateTime04.GetValueOrDefault();
            DateTime? dateTime06 = dateTime05;
            Console.WriteLine("Type Casting : ");
            Console.WriteLine(dateTime04);
            Console.WriteLine("GetValueOrDefault(): " + dateTime05);
            Console.WriteLine("GetValueOrDefault(): " + dateTime06.GetValueOrDefault());

            DateTime? dateTime07 = null;
            DateTime dateTime08;
            
            if(dateTime07 != null)
                dateTime08 = dateTime07.GetValueOrDefault();
            else 
                dateTime08 = DateTime.Today;
            Console.WriteLine("DateTime 08: " + dateTime08);


            DateTime dateTime09 = dateTime07 ?? DateTime.Today;
            DateTime dateTime10 = ((dateTime07 != null) ? dateTime07.GetValueOrDefault() : DateTime.Today.AddDays(2));
            Console.WriteLine();
            Console.WriteLine("DateTime 09: " + dateTime09);
            Console.WriteLine("DateTime 10: " + dateTime10);
        }
        static void Main(string[] args)
        {
            Basic01();

            Console.WriteLine();
            Console.WriteLine();

            Basic02();
        }
    }
}
