using System;
using System.Collections.Generic;

namespace _8._Working_With_Text
{
    internal class StringUtility
    {
        public static string summarizeText(string text, int maxLength=20)
        {
            if(text.Length < maxLength)
                return text;
            var words = text.Trim().Split(' ');
            var summaryWords = new List<string>();
            var len = 0;
            foreach(var word in words)
            {
                summaryWords.Add(word);
                len += word.Length + 1; // +1 for spaces
                if (len >= maxLength)
                    break;
            }
            return String.Join(" ", summaryWords) + "...";
        }
    }
}
