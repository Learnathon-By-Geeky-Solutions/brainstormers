namespace _02_Classes.Methods
{
    internal class Methods01
    {
        public static void run()
        {
            //var number = int.Parse("abc"); // will show error
            /*int number;
            var result = int.TryParse("abc", out number);
            if (result)
            {
                Console.WriteLine("Number is " + number);
            }
            else {
                Console.WriteLine("Conversion Failed");
            }*/

            // alternative
            try
            {
                var number = int.Parse("abc");
                Console.WriteLine("Number is " + number);
            }
            catch (Exception)
            {
                Console.WriteLine("Conversion Failed");
            }

        }
        public static void UseParams()
        {
            var calculator = new Calculator();
            Console.WriteLine(calculator.Add(1, 2));
            Console.WriteLine(calculator.Add(1, 2, 3));
            Console.WriteLine(calculator.Add(1, 2, 3, 4));
            Console.WriteLine(calculator.Add(new int[] { 1, 2, 3, 4 }));
        }
        public static void MethodsOverloadingWithNullChecking()
        {
            try
            {
                Console.WriteLine("\t\t02 OOP, 02, Classes, 04 Method");
                Console.WriteLine("\t\t\tMethods Overloading");

                var point = new Point(10, 20);
                Console.WriteLine("Points are x = {0}, y = {1}", point.X, point.Y);

                point.Move(new Point(100, 200));
                Console.WriteLine("Points are x = {0}, y = {1}", point.X, point.Y);

                //point.Move(null); // will show errors // under this line won't show, Program will crash

                point.Move(new Point(-15, -11));
                Console.WriteLine("Points are x = {0}, y = {1}", point.X, point.Y);

            }
            catch (Exception)
            {
                Console.WriteLine("An Unexpected Error Occured. 02 OOP -> 02 Classes -> Methods -> Methods.cs");
            }
        }
    }
}
