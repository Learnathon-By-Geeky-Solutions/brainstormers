using System;
using System.Collections.Generic;

namespace _9._Working_With_Files
{
    internal class DemoFileAndFileInfo
    {
        public void Learn()
        {
            //File (provides static methods)
            var pathFrom = @"C:\Users\Rajon\Desktop\New Text Document.txt";
            var pathTo = @"C:\Users\Rajon\Desktop\AnotherTextDocument.txt";
            if (File.Exists(pathFrom))
            {
                File.Copy(pathFrom, pathTo, true);
                File.Delete(pathTo);
                Console.WriteLine($"File Deleted. Path:{pathTo}");
            }

            //FileInfo (provides instance methods)
            var fileInfo = new FileInfo(pathFrom);
            if(fileInfo.Exists)
            {
                fileInfo.CopyTo(pathTo);
                fileInfo.Delete();
            }
        }
    }
}
