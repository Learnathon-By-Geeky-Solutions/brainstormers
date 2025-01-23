// Delegates : An object that knows how to call a method (or a group of methods)
// A reference to a function
// A delegate is a powerful type-safe mechanism for referencing methods.
using System.Diagnostics;

namespace Delegates
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var photoProcessor = new PhotoProcessor();
            //photoProcessor.Process("afsar.jpg");

            var process = new PhotoProcessor();

            var filters = new PhotoFilters();
            PhotoProcessor.PhotoFilterHandler filterHandler = filters.ApplyBrightness; // if delegate signature does not match, it throw a compile-time error.
            filterHandler += filters.ApplyContrast;
            filterHandler += RemoveRedEyeFilter;

            process.Process("photo.jpg", filterHandler);

            Console.WriteLine("\n");

            var process02 = new PhotoProcessor();

            var filters02 = new PhotoFilters();
            Action<Photo> filterHandler02 = filters02.ApplyBrightness;
            filterHandler02 += filters02.ApplyContrast;
            filterHandler02 += RemoveRedEyeFilter;

            process.Process("photo02.jpg", filterHandler);
        }

        static void RemoveRedEyeFilter(Photo photo)
        {
            Console.WriteLine("Apply RemoveRedEyeFilter");
        }
    }
}
