namespace _02_Classes.Methods
{
    public class Calculator
    {
        public int Add(params int[] numbers)
        {
            var sum = 0;
            foreach (var i in numbers)
            {
                sum += i;
            }
            return sum;
        }
    }
}
