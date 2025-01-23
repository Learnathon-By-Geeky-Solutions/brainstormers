using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2._Classes.Methods
{
    internal class Calculator
    {
        public int add(params int[] numbers)
        {
            var res = 0;
            foreach (var item in numbers)
            {
                res += item;
            }
            return res;
        }
    }
}
