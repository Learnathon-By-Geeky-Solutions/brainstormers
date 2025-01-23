namespace CSharpAdvanced.Generics;

// constraint to a Interface 
public class Utilities<T> where T : IComparable, new()
{
    public void DoSomething(T value)
    {
        var obj = new T(); // To instantiate T, we need to add new() at the first line of class
    }
    public T Max(T a, T b) // where T : IComparable (this is done at class level)
    {
        return a.CompareTo(b)>0 ? a : b;
    }
}