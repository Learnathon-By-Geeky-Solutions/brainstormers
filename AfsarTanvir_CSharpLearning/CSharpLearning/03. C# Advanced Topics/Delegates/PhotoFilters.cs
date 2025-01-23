// Delegates : An object that knows how to call a method (or a group of methods)
// A reference to a function
// A delegate is a powerful type-safe mechanism for referencing methods.
namespace Delegates
{
    public class PhotoFilters
    {
        //public PhotoFilters()
        //{
            
        //}
        public void ApplyBrightness(Photo photo)
        {
            Console.WriteLine("Apply Brightness");
        }
        public void ApplyContrast(Photo photo)
        {
            Console.WriteLine("Apply Contrast");
        }
        public void ApplyHue(Photo photo)
        {
            Console.WriteLine("Apply ");
        }
        public void Resize(Photo photo)
        {
            Console.WriteLine("Resize photo");
        }
    }
}
