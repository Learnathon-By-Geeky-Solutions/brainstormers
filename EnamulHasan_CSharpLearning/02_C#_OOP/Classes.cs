using System;

namespace Fundamentals
{
    public class Person
    {
        public Person()
        {

        }

        public Person(string firstName)
            : this()
        {
            this.FirstName = firstName;
        }

        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public void Intro()
        {
            Console.WriteLine("Name: " + FirstName + " " + LastName);
        }
    }

}

