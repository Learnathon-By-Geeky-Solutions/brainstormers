using System;

namespace Fundamentals
{
    public class Person
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public void Intro(){
            Console.WriteLine("Name:" + FirstName + " " + LastName);
        }
    }

}

