using System;

namespace _5._Control_Flow
{
    public enum Season
    {
        Summer,
        Autumn,
        RainySeason,
        Spring
    }
    public class SwitchCase
    {
        public void ChooseWhatToDo()
        {
            var season = Season.Summer;
            switch (season)
            {
                case Season.Summer:
                    Console.WriteLine("This is Summer");
                    break;

                case Season.Autumn:
                    Console.WriteLine("This is Autumn");
                    break;

                case Season.RainySeason:
                case Season.Spring: // works like "OR" RainySeason or Spring
                    Console.WriteLine("Cold Season");
                    break;

                default:
                    Console.WriteLine("Don't know this Season");
                    break;
            }
        }

    }
}
