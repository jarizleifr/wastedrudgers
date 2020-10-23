using ManulECS;

namespace WasteDrudgers
{
    [SerializationProfile("global")]
    public class Calendar
    {
        public Calendar(int year, int month, int day, int hour, int minute, float second)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
        }

        private readonly string[] monthStrings = new string[12]
        {
            "Newmoon", "Candlemoon", "Birthmoon",
            "Sapmoon", "Milkmoon", "Flowermoon",
            "Grassmoon", "Harvestmoon", "Winemoon",
            "Mistmoon", "Deathmoon", "Wintermoon"
        };

        private readonly string[] dayStrings = new string[7]
        {
            "Sundath", "Moondath", "Faidath", "Treidath", "Praydath", "Frendath", "Wasdath",
        };

        private readonly string[] ordinalStrings = new string[4]
        {
            "st", "nd", "rd", "th"
        };

        private readonly string[] timeOfDay = new string[]
        {
            "Dusk",
            "Night",
            "Midnight",
            "Twilight",
            "Dawn",
            "Morning",
            "Noon",
            "Afternoon",
            "Evening"
        };

        private int year;
        private int month;
        private int day;
        private int hour;
        private int minute;
        private float second;
        private double previousTime;

        private int ticks = 0;

        private void RecalculateCalendar()
        {
            while (second >= 60) { second -= 60; minute++; }
            while (minute >= 60) { minute -= 60; hour++; }
            while (hour >= 24) { hour -= 24; day++; }
            while (day > 28) { day -= 28; month++; }
            while (month > 12) { month -= 12; year++; }

            double timeDifference = CurrentTimeDouble() - previousTime;
        }

        public void PassDate(int year, int month, int day)
        {
            previousTime = CurrentTimeDouble();
            this.day += day;
            this.month += month;
            this.year += year;
            RecalculateCalendar();
        }

        public void PassTime(int hour, int minute, float second)
        {
            previousTime = CurrentTimeDouble();
            this.second += second;
            this.minute += minute;
            this.hour += hour;
            RecalculateCalendar();
        }

        public int Hour => hour;

        private double CurrentTimeDouble()
        {
            double currentTime = 0;
            currentTime += year * 525600;
            currentTime += month * 40320;
            currentTime += day * 1440;
            currentTime += hour * 60;
            currentTime += minute;
            currentTime += second / 60;
            return currentTime;
        }

        public string GetDayString() => $"{GetWeekday(day)}, the {day}{GetOrdinal(day)}";
        public string GetMonthAndYearString() => $"of {monthStrings[month - 1]}, {year}";
        public string GetTimeString() =>
            $"{WithLeadingZero(hour)}:{WithLeadingZero(minute)}";

        public string WithLeadingZero(int value) => $"{(value > 9 ? "" : "0")}{value}";

        public string GetWeekday(int day) => dayStrings[day % 7];

        public string GetOrdinal(int day) => day switch
        {
            var _ when day > 3 && day < 21 => "th",
            var _ when day % 10 == 1 => "st",
            var _ when day % 10 == 2 => "nd",
            var _ when day % 10 == 3 => "rd",
            _ => "th",
        };

        public string GetDateString()
        {
            string d;
            if (day < 4) { d = day + ordinalStrings[day - 1]; } else { d = day + ordinalStrings[3]; }

            int weekday = day;
            while (weekday >= 8) { weekday -= 7; }

            string date = dayStrings[weekday - 1] + ", " + d + " of " + monthStrings[month - 1] + ", " + year;
            return date;
        }
    }
}