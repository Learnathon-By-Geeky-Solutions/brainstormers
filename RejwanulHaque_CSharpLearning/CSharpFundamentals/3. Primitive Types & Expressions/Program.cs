using System;

namespace _3._Primitive_Types___Expressions
{
    class Program
    {
        static void Main(string[] args)
        {
            TypeConversion tc = new TypeConversion();
            tc.ImplicitTypeConversion();
            tc.ExplicitTypeConversion();
            tc.NonCompatibleTypeConversion();

            Console.WriteLine();

            VariablesAndContants vc = new VariablesAndContants();
            vc.DataTypes();
        }
    }
}