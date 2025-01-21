using System;

namespace  Delegates
{
    public class Photo
    {

    }

    public class PhotoProcessor
    {
        public void Process(string path, Action<Photo> filterHandler)
        {
            var photo = new Photo();

            filterHandler(photo);

            //photo.save();
        }
    }

    public class PhotoFilters
    {
        public void Bright(Photo photo)
        {
            Console.WriteLine("Apply Bright!!");
        }

        public void Dark(Photo photo)
        {
            Console.WriteLine("Apply Dark!!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var processor = new PhotoProcessor();
            var filters = new PhotoFilters();
            
            Action<Photo> filterHandler = filters.Bright;
            //filterHandler += filters.Dark;

            processor.Process("photo", filterHandler);
        }
    }
}