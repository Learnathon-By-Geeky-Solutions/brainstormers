namespace _04_Non_Primitive_Types
{
    internal class Arrays01
    {
        public static void run()
        {
            int[] numbers01 = new int[3];
            numbers01[0] = 1;
            numbers01[1] = 2;
            numbers01[2] = 3;

            for (int i = 0; i < numbers01.Length; i++)
            {
                Console.Write(numbers01[i] + (i + 1 == numbers01.Length ? "\n" : " "));
            }

            var numbers02 = new int[3];
            numbers02[0] = 10;
            numbers02[1] = 20;
            numbers02[2] = 30;

            for (int i = 0; i < numbers02.Length; i++)
            {
                Console.Write(numbers02[i] + (i + 1 == numbers02.Length ? "\n" : " "));
            }

            var numbers03 = new int[] {5, 6, 7};

            for (int i = 0; i < numbers03.Length; i++)
            {
                Console.Write(numbers03[i] + (i + 1 == numbers03.Length ? "\n" : " "));
            }
        }
    }
}
