using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5._Control_Flow
{
    public class PasswordGenerator
    {
        Random random = new Random();
        public void Generate(int passwordLength)
        {
            var buffer = new char[passwordLength];
            for(var i=0;i<passwordLength;i++)
            {
                buffer[i] = (char)('a' + random.Next(0, 26));
            }
            string password = new string(buffer);
            Console.WriteLine($"Password is: {password}");
        }
    }
}
