namespace _04_Inheritance.Upcasting_and_Downcasting
{
    internal class Class01
    {
        public static void run()
        {
            Text01 text01 = new Text01();
            Shape01 shape01 = text01;

            shape01.Draw(); shape01.X = 10; shape01.Y = 11; shape01.Width = 12; shape01.Height = 13;
            text01.Draw(); text01.X = 20; text01.Y = 21; text01.Width = 22; text01.Height = 23; text01.FontSize = 24; text01.FontName = "Nothing";

            //StreamReader reader01 = new StreamReader(new FileStream());
            //StreamReader reader02 = new StreamReader(new MemoryStream()); // type casting

            //ArrayList list01 = new ArrayList();
            //list01.Add(1);
            //list01.Add("Mosh");
            //list01.Add(new Text01());

            //var list02 = new List<Shape01>();

            Shape01 shape02 = new Text01();
            Text01 text02 = (Text01) shape02;
        }
    }
}
