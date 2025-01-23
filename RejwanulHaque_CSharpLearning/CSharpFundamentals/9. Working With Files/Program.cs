using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace _9._Working_With_Files
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("DemoFileAndFileInfo: ");
            DemoFileAndFileInfo demoFileAndFileInfo = new DemoFileAndFileInfo();
            demoFileAndFileInfo.Learn();

            Console.WriteLine();
            Console.WriteLine("DemoDirectoryAndDirectoryInfo: ");

            DemoDirectoryAndDirectoryInfo directoryAndDirectoryInfo = new DemoDirectoryAndDirectoryInfo();
            directoryAndDirectoryInfo.Learn();

            Console.WriteLine();
            Console.WriteLine("DemoPath: ");

            DemoPath demoPath = new DemoPath();
            demoPath.Learn();

        }
    }
}
