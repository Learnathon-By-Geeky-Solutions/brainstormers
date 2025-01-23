using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5._Polymorphism.MethodOverriding
{
    internal class MethodOverridingDemo
    {
        public static void Learn()
        {
            var shapes = new List<Shape>();
            shapes.Add(new Circle());
            shapes.Add(new Rectangle());
            shapes.Add(new Triangle());

            Canvas.DrawShapes(shapes);
        }
    }
}
