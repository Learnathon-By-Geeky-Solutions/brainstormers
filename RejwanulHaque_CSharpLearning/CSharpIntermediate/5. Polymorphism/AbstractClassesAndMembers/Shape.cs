namespace _5._Polymorphism.AbstractClassesAndMembers;

public abstract class Shape
{
    public int Width { get; set; }
    public int Height { get; set; }

    public abstract void Draw();
    public void Copy()
    {
        Console.WriteLine("Shape copied to clipboard.");
    }
}