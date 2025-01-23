using System;
using System.Collections.Generic;

namespace _2._Classes.PropertiesDemoFolder
{
    public class Person
    {
        //public string Name { get; set; }
        public DateTime Birthdate { get; private set; }
        public Person(DateTime birthdate)
        {
            Birthdate = birthdate;
        }

        public int Age
        {
            get
            {
                var timespan = DateTime.Today - Birthdate;
                var years = timespan.Days / 365;
                return years;
            }
        }
    }
}
