namespace Generics
{
    public class UtilitiesBasic
    {
        public int Max01(int a, int b)
        {
            return a > b ? a : b;
        }
        public T Max02<T> (T a, T b) where T : IComparable
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

    }
}
