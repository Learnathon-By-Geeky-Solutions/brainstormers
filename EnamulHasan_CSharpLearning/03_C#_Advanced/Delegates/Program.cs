using System;

namespace  Delegates
{
    public class Photo
    {
        public required string Path { get; set; }
        public DateTime DateTaken { get; set; }

        public static void Save()
        {
            Console.WriteLine("Photo saved.");
        }
    }

    public static class PhotoProcessor
    {
        public static void Process(string path, Action<Photo> filterHandler)
        {
            var photo = new Photo { Path = path, DateTaken = DateTime.Now };

            filterHandler(photo);

            Photo.Save();
        }
    }

    public static class PhotoFilters
    {
        public static void Bright(Photo photo)
        {
            Console.WriteLine("Apply Bright!!");
        }

        public static void Dark(Photo photo)
        {
            Console.WriteLine("Apply Dark!!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Action<Photo> filterHandler = PhotoFilters.Bright;
            filterHandler += PhotoFilters.Dark;

            PhotoProcessor.Process("photo", filterHandler);
        }
    }
}