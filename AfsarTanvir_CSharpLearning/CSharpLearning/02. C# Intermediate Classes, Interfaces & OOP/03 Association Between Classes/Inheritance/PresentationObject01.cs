namespace _03_Association_Between_Classes.Inheritance
{
    internal class PresentationObject01
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public void Copy()
        {
            Console.WriteLine("Object copied to chipboard.");
        }
        public void Duplicate()
        {
            Console.WriteLine("object was duplicated.");
        }
    }
}
