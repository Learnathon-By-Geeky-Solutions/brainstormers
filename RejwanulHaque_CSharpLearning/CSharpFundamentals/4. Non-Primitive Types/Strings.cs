using System;

namespace _4._Non_Primitive_Types
{
    public class Strings
    {
        public string firstName;
        public string lastName;

        public void Introduce()
        {
            //string name = firstName + " " + lastName;
            string name = string.Format("{0} {1}", firstName, lastName); // both are correct 

            Console.WriteLine($"I am {name}");
        }

        public void StringFromArray()
        {
            var friends = new string[3] { "Jack", "Alice", "Bob" };
            string listOfFriends = string.Join(", ", friends);
            Console.WriteLine($"Friends of {firstName} {lastName} are {listOfFriends}");
        }

        public void PrintVerbatimString()
        {
            //string path = "c:\\projects\\project1\\folder1";
            string path = @"c:\projects\project1\folder1"; // both are correct
            Console.WriteLine($"Path of a project: {path}");

            Console.WriteLine("3 Paths: ");
            string paths = @"c:\projects\project1\folder1
c:\projects\project1\folder2
c:\projects\project1\folder3";
            Console.WriteLine(paths);
        }
    }
}
