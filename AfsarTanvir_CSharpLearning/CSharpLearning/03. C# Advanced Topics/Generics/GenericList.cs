namespace Generics
{
    public class GenericList<T>
    {
        private readonly List<T> _items = new List<T>();

        public void Add(T value)
        {
            _items.Add(value);
            Console.WriteLine($"Item added: {value}");
        }
        public T this[int index]
        {
            get { throw new NotImplementedException(); }
        }
    }
}
