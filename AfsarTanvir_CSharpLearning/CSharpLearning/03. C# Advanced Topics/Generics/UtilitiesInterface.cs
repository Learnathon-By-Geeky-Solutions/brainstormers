namespace Generics
{
    // where T : ICompareable 
    // where T : Product
    // where T : struct 
    // where T : class
    // where T : new()
    public class UtilitiesInterface<T> where T : IComparable, new()
    {
        public int Max01(int a, int b)
        {
            return a > b ? a : b;
        }
        public T Max02(T a, T b)
        {
            return a.CompareTo(b) > 0 ? a : b;
        }
        public void DoSomething(T value)
        {
            var obj = new T();
        }
    }
}
