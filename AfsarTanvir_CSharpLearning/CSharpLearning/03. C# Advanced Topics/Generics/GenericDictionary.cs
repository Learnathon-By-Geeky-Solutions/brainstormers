namespace Generics
{
    // we can use Generics, so that we don't have do create List class for every class
    //public class ObjectList01
    //{
    //    public void Add(object obj)
    //    {

    //    }
    //    public object this[int index]
    //    {
    //        get { throw new NotImplementedException(); }
    //    }
    //}
    public class GenericDictionary<Tkey, Tvalue>
    {
        private readonly Dictionary<Tkey, Tvalue> _dictionary = new Dictionary<Tkey, Tvalue>();

        public void Add(Tkey key, Tvalue value)
        {
            _dictionary[key] = value;
        }
    }
}
