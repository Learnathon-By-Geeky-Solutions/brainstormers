namespace _04_Non_Primitive_Types
{
    internal class Strings01
    {
        public static void run()
        {
            var FirstName = "Afsar";
            string LastName = "Tanvir";

            var FullName01 = FirstName + " " + LastName; 
            var FullName02 = string.Format("{0} {1}", FirstName, LastName);
            Console.WriteLine(FullName01);
            Console.WriteLine(FullName02);

            var names = new string[4] { "Rajon", "Enamul", "Opu", "Bodrul" };
            var formattedNames = string.Join(", ", names);
            Console.WriteLine(formattedNames);

            var text01 = "Hi John\nLook into the following paths\nc:\\folder1\\folder02\tc:\\folder2\\folder01";
            Console.WriteLine(text01);

            var text02 = @"Hi John
Look into the following paths
c:\folder1\folder02    c:\folder2\folder01";
            Console.WriteLine(text02);
        }
    }
}
