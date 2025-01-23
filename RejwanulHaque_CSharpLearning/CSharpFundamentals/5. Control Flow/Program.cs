using System;

namespace _5._Control_Flow
{
    class Program
    {
        public static void Main(string[] args)
        {
            SwitchCase switchCase = new SwitchCase();
            switchCase.ChooseWhatToDo();

            Console.WriteLine();
            Console.WriteLine("ForEachLoop Class: ");

            var array = new int[3] { 1, 2, 5 };
            ForEachLoop forEachLoop = new ForEachLoop();
            forEachLoop.PrintArray(array);

            Console.WriteLine();
            Console.WriteLine("WhileLoop: ");

            WhileLoop whileLoop = new WhileLoop();
            whileLoop.TakeInput();

            Console.WriteLine();
            Console.WriteLine("PasswordGenerator: ");

            PasswordGenerator passwordGenerator = new PasswordGenerator();
            passwordGenerator.Generate(10);
        }
    }
}
