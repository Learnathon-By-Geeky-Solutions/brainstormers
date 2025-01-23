namespace _02_Classes
{
    public class Person02
    {
        private DateTime _birthdate;

        public void SetBirthdate(DateTime birthdate)
        {
            this._birthdate = birthdate;
        }
        public DateTime GetBirthdate()
        {
            return this._birthdate;
        }
    }
    internal class AccessModifiers01
    {
        public static void run()
        {
            var person02 = new Person02();
            person02.SetBirthdate(new DateTime(2024, 01, 08));
            Console.WriteLine("Birthdate : " + person02.GetBirthdate());
        }
    }
}
