using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9._Working_With_Files
{
    internal class DemoDirectoryAndDirectoryInfo
    {
        public void Learn()
        {
            Directory.CreateDirectory(@"C:\Users\Rajon\Desktop\NewFolderCreatedByDirectoryClass.txt");

            var files = Directory.GetFiles(@"C:\Users\Rajon\Desktop", "*.txt", SearchOption.TopDirectoryOnly);
            //var files = Directory.GetFiles(@"C:\Users\Rajon\Desktop", "*.txt", SearchOption.AllDirectories);
            Console.WriteLine("txt files in Desktop: ");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
            Console.WriteLine();
            Console.WriteLine("Directories inside: C:\\Users\\Rajon\\Desktop");
            var directories = Directory.GetDirectories(@"C:\Users\Rajon\Desktop", "*.*", SearchOption.TopDirectoryOnly);
            foreach (var directory in directories)
                Console.WriteLine(directory);


            // Directory Info
            var directoryInfo = new DirectoryInfo(@"C:\Users\Rajon\Desktop");
            //Works same as Directory

        }
    }
}
