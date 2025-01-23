namespace _5._Polymorphism.MethodOverriding;

public class Canvas
{
    public static void DrawShapes(List<Shape> shapes)
    {
        foreach (var shape in shapes)
        {
            shape.draw();
        }
    }
}