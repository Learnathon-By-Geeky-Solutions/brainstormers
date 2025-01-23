// Delegates : An object that knows how to call a method (or a group of methods)
// A reference to a function
// A delegate is a powerful type-safe mechanism for referencing methods.
namespace Delegates
{
    public class Photo
    {
        public static Photo Load(string path)
        {
            return new Photo();
        }
        public void Save()
        {
            Console.WriteLine("Photo Save");
        }
    }
}
