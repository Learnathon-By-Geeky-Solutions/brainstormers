using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5._Control_Flow
{
    public class WhileLoop
    {
        public void TakeInput()
        {
            while (true)
            {
                Console.WriteLine("Enter input: ");
                var input = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(input))
                {
                    break;
                }
                Console.WriteLine($"You entered {input}");
            }
        }
    }
}
