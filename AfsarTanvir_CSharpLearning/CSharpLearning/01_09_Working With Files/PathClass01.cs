namespace _01_09_Working_With_Files
{
    internal class PathClass01
    {
        public static void run()
        {
            var path01 = @"C:\Users\t440s\Documents\cSharp Project\CSharp_Learning\CSharpLearning\LearnCSharp\LearnCSharp.sln";

            var dotIndex01 = path01.IndexOf('.');
            var extension01 = path01.Substring(dotIndex01);

            Console.WriteLine($"Extension = {extension01}");
            Console.WriteLine($"File Name = {Path.GetFileName(path01)}");
            Console.WriteLine($"File Name Without Extension = {Path.GetFileNameWithoutExtension(path01)}");
            Console.WriteLine($"Directory Name = {Path.GetDirectoryName(path01)}");
        }
    }
}
