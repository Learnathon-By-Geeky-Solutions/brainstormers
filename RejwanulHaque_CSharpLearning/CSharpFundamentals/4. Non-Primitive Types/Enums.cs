using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4._Non_Primitive_Types
{
    /*

     * Enums in C# are a way to define a group of named constants, 
     * making your code more readable and maintainable. 
     * They are particularly useful when you have a set of related 
     * values that are fixed and won't change.
  
     */

    public enum ShippingMethod
    {
        RegularAirMail = 1, RegisteredAirMail = 2, Express = 3
    }

    public enum Level : byte
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum Status
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum Day
    {
        Saturday, // by default first one is 0
        Sunday,   // second one is 1 and so on 
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday
    }

    public enum Month
    {
        January=1, 
        February,

        October = 10,
        November, // 11
        December  // 12
    }

    public class Enums
    {
        public void ShippingMethodCheck()
        {
            var method = ShippingMethod.Express;
            Console.WriteLine($"Method No. {(int)method} is {method.ToString()}");

            var methodId = 2;
            Console.WriteLine($"The Shipping Method No. {methodId} is: {(ShippingMethod)methodId}");

        }
        public void ShowDayName()
        {
            var today = Day.Tuesday;
            Console.WriteLine($"Today is {today}");
            Console.WriteLine($"Today is {(int)today}th day of the week");
        }

        public void MakeShippingMethod(string method)
        {
            var shippingMethod = (ShippingMethod) Enum.Parse(typeof (ShippingMethod ), method );
            Console.WriteLine($"The shipping method is : {shippingMethod.ToString()}");
        }
        public void MakeDay(string method)
        {
            var day = (Day) Enum.Parse(typeof (Day), method );
            Console.WriteLine($"The day is : {day.ToString()}");
        }

        public void PrintMonths()
        {
            foreach (var value in Enum.GetValues(typeof(Month)))
            {
                Console.WriteLine($"{value}: {(int)value}");
            }
        }
    }
}
