namespace _05_Polymorphism.Abstract_Classes_and_Members
{
    public abstract class Shape02
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public abstract void Draw();

        public void copy()
        {
            Console.WriteLine("Copy shape into clipboard.");
        }
        public void Select()
        {
            Console.WriteLine("Select the shape.");
        }
    }
}
