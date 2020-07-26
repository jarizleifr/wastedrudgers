using System;

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
    }
}