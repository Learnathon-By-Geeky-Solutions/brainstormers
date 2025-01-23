namespace CSharpAdvanced.ExceptionHandling
{
    public class ExceptionHandling1()
    {
        public static void Run()
        {
            try
            {
                var calculator = new Calculator01();

                var result = calculator.Divide(5, 0);

                // Case 2: Invalid argument (ArgumentException)
                // var result = calculator.Divide(-5, 2);

                // Case 3: No exception
                //var result = calculator.Divide(10, 2);

                // Case 4: Unexpected exception (simulate any other error)
                // throw new Exception("Unexpected error occurred.");


                Console.WriteLine($"Result: {result}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine("Caught DivideByZeroException: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Caught ArgumentException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught General Exception: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Execution of Run1 completed. Cleaning up resources if any.");
            }
        }
    }
}
