namespace _01_08_Working_With_Text
{
    internal class Procedural_Programming01
    {
        public static void run()
        {
            Console.Write("Please Enter Your Name: ");
            var name01 = Console.ReadLine();
            string reversed01 = ReverseName(name01);
            Console.WriteLine(reversed01);
        }
        public static string ReverseName(string name) {
            var array = new char[name.Length];
            for(var i=name.Length; i>0; i--)
            {
                array[name.Length-i] = name[i-1];
            }
            return new string(array);
        }
    }
}
