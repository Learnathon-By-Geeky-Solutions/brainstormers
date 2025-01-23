namespace ExceptionHandling
{
    public class Calculator01
    {
        // This method divides two numbers. Throws exceptions based on input.
        public int Divide(int numerator, int denominator)
        {
            if (denominator == 0)
                throw new DivideByZeroException("Denominator cannot be zero.");
            if (numerator < 0 || denominator < 0)
                throw new ArgumentException("Negative values are not allowed.");
            return numerator / denominator;
        }
    }
}
