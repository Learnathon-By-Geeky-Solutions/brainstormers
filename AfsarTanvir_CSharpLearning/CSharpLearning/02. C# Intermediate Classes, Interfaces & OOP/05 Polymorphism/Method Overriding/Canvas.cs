namespace _05_Polymorphism.Method_Overriding
{
    public class Canvas
    {
        public void DrawShapes(List<Shape01> shapes)
        {
            foreach (var shape in shapes)
            {
                shape.Draw();
            }
        }
    }
}
