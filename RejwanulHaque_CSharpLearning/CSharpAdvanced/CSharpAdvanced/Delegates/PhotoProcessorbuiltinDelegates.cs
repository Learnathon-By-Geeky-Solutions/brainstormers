namespace CSharpAdvanced
{
    public class PhotoProcessorbuiltinDelegates
    {
        //public delegate void PhotoFilterHandler(Photo photo);  // this is not needed now

        public void Process(string path, Action<Photo> filterHandler)
        {
            var photo = Photo.Load(path);

            filterHandler(photo);

            photo.save();
        }
    }
}

/* 
 * 
 * There are 2 types of builtin delegates: 
 * 
 * System.Action: points to the functions that returns nothing (void). 2 forms-> nongeneric and generic
 * System.Func: point to the functions that returns something. same 2 forms. 
 * 
 */