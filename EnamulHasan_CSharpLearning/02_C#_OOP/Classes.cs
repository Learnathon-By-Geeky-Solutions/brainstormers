using System;

namespace Fundamentals
{
    public class Person
    {
        public Person(){

        }

        public Person(string firstName)
            : this()
        {
            this.FirstName = firstName; 
        }

        public string FirstName;
        public string LastName;
        public void Intro(){
            Console.WriteLine("Name: " + FirstName + " " + LastName);
        }
    }

}

