// A set of name/ values pairs(constant)
namespace _04_Non_Primitive_Types
{
    public enum ShippingMethod
    {
        // if we do not intialize the values, first value is 0, then it will increase by 1
        RehularAirMail = 1,
        RegisteredAirMail = 2,
        Express = 3
    }
    public enum Month
    {
        January = 1, February, March, April, May, June, July, Augest, October, November=20, December
    }
    class Enum01
    {
        public static void run()
        {
            var method01 = ShippingMethod.Express;
            Console.WriteLine((int) method01);

            var method02 = 2;
            Console.WriteLine((ShippingMethod) method02);

            var method03 = 3;
            Console.WriteLine((Month) method03);

            var method04 = Month.October;
            Console.WriteLine(method04);
            Console.WriteLine(method04.ToString());

            var method05 = "December";
            var monthMethod05 = (Month) Enum.Parse(typeof(Month), method05);
            Console.WriteLine(method05);
            // Console.WriteLine((int) method05); // Will show error, string can not convert int
            Console.WriteLine(monthMethod05);
            Console.WriteLine((int) monthMethod05);
        }
    }
}
