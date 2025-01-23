namespace _01_09_Working_With_Files
{
    internal class File_and_FileInfo01
    {
        public static void run()
        {
            // File.Copy("c:\\temp\\myfile.jpg", "c:\\temp\\myfile.jpg", true);
            File.Copy(@"c:\temp\myfile.jpg", @"c:\temp\myfile.jpg", true);

            var path = @"c:\somefile.jpg";
            File.Delete(path);
            if (File.Exists(path))
            {
                //
            }
            var content01 =  File.ReadAllText(path);
            var fileInfo01 = new FileInfo(content01);
            fileInfo01.CopyTo("...");
            fileInfo01.Delete();
            if (fileInfo01.Exists) {
                //
            }
            
        }
    }
}
