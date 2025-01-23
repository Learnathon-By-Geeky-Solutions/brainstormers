namespace _02_Classes
{
    public class Person03
    {
        public string Name {  get; set; }
        public string UserName {  get; set; }
        public DateTime Birthdate {  get; private set; }

        public Person03(DateTime dateTime)
        {
            Birthdate = dateTime;
        }
        public int Age
        {
            get
            {
                var timeSpan = DateTime.Now - Birthdate;
                var years = timeSpan.Days / 365;

                return years;
            }
        }

    }
    internal class Properties01
    {
        public static void run()
        {
            var person = new Person03(new DateTime(2012, 02, 25));
            //person.Birthdate = new DateTime(2012, 02, 25);

            Console.WriteLine("You are " + person.Age + " years old.");
        }
    }
}
