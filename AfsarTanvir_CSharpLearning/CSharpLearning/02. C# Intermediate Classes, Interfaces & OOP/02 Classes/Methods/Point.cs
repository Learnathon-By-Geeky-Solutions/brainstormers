namespace _02_Classes.Methods
{
    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public void Move(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public void Move(Point p)
        {
            if(p == null)
            {
                throw new ArgumentNullException("New Location", "Error in 02 OOP -> 02 Classes -> Methods -> Points.cs");
            }
            Move(p.X, p.Y);

            // alternative
            //this.X = p.X;
            //this.Y = p.Y;
        }
    }
}
