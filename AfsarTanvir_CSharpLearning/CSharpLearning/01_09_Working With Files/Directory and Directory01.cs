namespace _01_09_Working_With_Files
{
    internal class Directory_and_Directory01
    {
        public static void run()
        {
            Directory.CreateDirectory(@"c:\temp\folder1");

            // this will shows whats in the folder like videos, picture, subfolder etc
            //var files =  Directory.GetFiles(@"C:\Users\t440s\Videos", "*.*", SearchOption.AllDirectories);
            //var files =  Directory.GetFiles(@"C:\Afsar Tanvir\Semester 7", "*.*", SearchOption.AllDirectories);

            // show only vs solution file
            var files = Directory.GetFiles(@"C:\Users\t440s\Documents\cSharp Project\CSharp_Learning\CSharpLearning\LearnCSharp", "*.sln", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            // show any directories
            var directories01 = Directory.GetDirectories(@"C:\Users\t440s\Documents\cSharp Project\CSharp_Learning\CSharpLearning\LearnCSharp", "*.*", SearchOption.AllDirectories);
            foreach (var directory in directories01)
            {
                Console.WriteLine(directory);
            }

            // show if the directory exists or not 
            Directory.Exists("...");

            var directoryInfo01 = new DirectoryInfo("...");
            directoryInfo01.GetFiles();
            directoryInfo01.GetDirectories();
        }
    }
}
