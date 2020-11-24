using ManulECS;

namespace WasteDrudgers
{
    [SerializationProfile("global")]
    public class Calendar
    {
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
        private int second;

        private int ticks = 0;

        public Calendar(int year, int month, int day, int hour, int minute, int second)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
        }

        private void RecalculateCalendar()
        {
            while (second >= 60) { second -= 60; minute++; }
            while (minute >= 60) { minute -= 60; hour++; }
            while (hour >= 24) { hour -= 24; day++; }
            while (day >= 28) { day -= 28; month++; }
            while (month >= 12) { month -= 12; year++; }
        }

        public void PassDate(int year, int month, int day)
        {
            this.day += day;
            this.month += month;
            this.year += year;
            RecalculateCalendar();
        }

        public void PassTime(int hour, int minute, int second)
        {
            this.second += second;
            this.minute += minute;
            this.hour += hour;
            RecalculateCalendar();
        }

        public int Hour => hour;

        public string GetDayString() => $"{GetWeekday(day)}, the {day + 1}{GetOrdinal(day)}";
        public string GetMonthAndYearString() => $"of {monthStrings[month]}, {year}";
        public string GetTimeString() =>
            $"{WithLeadingZero(hour)}:{WithLeadingZero(minute)}";

        public string WithLeadingZero(int value) => $"{(value > 9 ? "" : "0")}{value}";

        public string GetWeekday(int day) => dayStrings[day % 7];

        public string GetOrdinal(int day) => day switch
        {
            var _ when day > 3 && day < 21 => ordinalStrings[0],
            var _ when day % 10 == 1 => ordinalStrings[1],
            var _ when day % 10 == 2 => ordinalStrings[2],
            var _ when day % 10 == 3 => ordinalStrings[3],
            _ => ordinalStrings[0],
        };
    }
}