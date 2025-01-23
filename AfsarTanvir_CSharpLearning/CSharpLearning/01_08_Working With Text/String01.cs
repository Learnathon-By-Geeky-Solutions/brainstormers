namespace _01_08_Working_With_Text
{
    internal class String01
    {
        public static void run()
        {
            // Trim // Toupper
            var fullName01 = " Afsar Tanvir ";
            Console.WriteLine(fullName01);
            Console.WriteLine("Trim: {0}", fullName01.Trim()); // Original Variable do not change
            Console.WriteLine("Trim and ToUpper: {0}", fullName01.Trim().ToUpper());

            //IndexOf
            var fullName02 = fullName01.Trim();
            var index01 = fullName02.IndexOf(' ');
            var firstName01 = fullName02.Substring(0, index01);
            var lastName01 = fullName02.Substring(index01+1);
            Console.WriteLine(firstName01);
            Console.WriteLine(lastName01);

            // Split
            var names01 = fullName02.Split(' ');
            Console.WriteLine("First Name : " + names01[0]);
            Console.WriteLine("Second Name : " + names01[1]);

            // Replace
            var fullName03 = fullName02.Replace("Tanvir", "TANVIR");
            Console.WriteLine(fullName03);

            // what if customer enter a null string
            if (String.IsNullOrEmpty(null))
                Console.WriteLine("Invalid String 01");
            if (String.IsNullOrEmpty(""))
                Console.WriteLine("Invalid String 02");
            if (String.IsNullOrEmpty(" "))
                Console.WriteLine("Invalid String 03");
            if (String.IsNullOrEmpty(" ".Trim()))
                Console.WriteLine("Invalid String 04");
            if (String.IsNullOrWhiteSpace(" "))
                Console.WriteLine("Invalid String 05");

            var str = "25";
            var age = Convert.ToByte(str);
            Console.WriteLine(age);

            float price = 125425.94f;
            Console.WriteLine(price.ToString("C"));
            Console.WriteLine(price.ToString("C0"));
        }
    }
}
