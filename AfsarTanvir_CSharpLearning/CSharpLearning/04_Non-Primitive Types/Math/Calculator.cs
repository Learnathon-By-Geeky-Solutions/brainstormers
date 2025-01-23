namespace _04_Non_Primitive_Types.Math
{
    public class Calculator
    {
        public int Add(int x, int y) { return x + y; }
        public int Subtract(int x, int y) {return x - y; }
        public int Multiply(int x, int y) { return x*y; }
        public double Divide(int x, int y) { if(y==0) return 0.0; return (double) x/y; }
    }
}
