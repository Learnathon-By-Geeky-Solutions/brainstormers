using System;

namespace Fundamentals
{
    public class Person
    {
        public string FirstName;
        public string LastName;
        public void Intro(){
            Console.WriteLine("Name:" + FirstName + " " + LastName);
        }
    }

}

