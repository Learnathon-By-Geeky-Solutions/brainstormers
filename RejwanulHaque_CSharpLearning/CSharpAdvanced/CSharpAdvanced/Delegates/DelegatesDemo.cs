using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpAdvanced.Delegates
{
    internal class DelegatesDemo
    {
        public static void Learn()
        {
            var processor = new PhotoProcessor();
            var filters = new PhotoFilters();
            PhotoProcessor.PhotoFilterHandler filterHandler = filters.ApplyBrightness;
            filterHandler += filters.ApplyContrast; // multicast delegate
            filterHandler += RemoveRedEyeFilter;
            processor.Process("Photo.jpg", filterHandler);



            // Using builtin Delegates
            Console.WriteLine();
            Console.WriteLine("Using Builtin Delegates: ");
            Console.WriteLine();

            var processor1 = new PhotoProcessorbuiltinDelegates();
            Action<Photo> filterHandler1 = filters.ApplyBrightness;
            filterHandler1 += filters.ApplyContrast;
            filterHandler1 += RemoveRedEyeFilter;

            processor1.Process("Photo.jpg", filterHandler1);
        }

        // Adding new filters (Extensibility) 
        static void RemoveRedEyeFilter(Photo photo)
        {
            Console.WriteLine("Apply RemoveRedEye");
        }
    }
}
