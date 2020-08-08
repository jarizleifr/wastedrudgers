using System;
using System.Linq;

namespace WasteDrudgers
{
    public static class StringUtils
    {
        public static string ModifierToString(int modifier) => (modifier >= 0 ? "+" : "") + modifier.ToString();

        public static string AverageDamageToString(float average)
        {
            var value = (int)average;
            return value.ToString() + ((int)Math.Round(average * 10) % 5 == 0 ? "½" : "");
        }

        public static string CapitalizeString(string input) => input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };
    }
}