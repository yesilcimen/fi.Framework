using System;
using System.Globalization;

namespace fi.Common
{
    public static class DateTimeExtension
    {
        public static string ToMonthName(this DateTime dateTime) => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        public static string ToShortMonthName(this DateTime dateTime) => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        public static bool IsNullOrEmpty(this DateTime target) => target.Equals(DateTime.MinValue);
        public static bool IsNotNullOrEmpty(this DateTime target) => !target.IsNullOrEmpty();
        public static bool IsNullOrEmpty(this DateTime? target) => (target is null || target.IsNullOrEmpty());
        public static bool IsNotNullOrEmpty(this DateTime? target) => !target.IsNullOrEmpty();
        public static string ToUniversalDateTimeString(this DateTime value) => $"{value:u}";
        public static DateTime GetFirstDayOfMonth(this DateTime dateTime) => new(dateTime.Year, dateTime.Month, 1);
        public static bool IsValidBirthday(this DateTime dateTime, int maxAge, int minAge) => dateTime >= DateTime.Today.AddYears(maxAge * -1) && dateTime < DateTime.Today.AddYears(minAge * -1);
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType) => new TimeSpan(Convert.ToInt64(Math.Round(time.Ticks / (decimal)roundingInterval.Ticks, roundingType)) * roundingInterval.Ticks);
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval) => time.Round(roundingInterval, MidpointRounding.AwayFromZero);
        public static DateTime RoundUp(this DateTime dt, TimeSpan d) => new((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks);
        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval) => new DateTime((datetime - DateTime.MinValue).Round(roundingInterval, MidpointRounding.AwayFromZero).Ticks);
        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            var nextMonthDate = dateTime.AddMonths(1);
            DateTime firstDayOfNextMonth = new(nextMonthDate.Year, nextMonthDate.Month, 1);
            var lastDayOfMonth = firstDayOfNextMonth.AddDays(-1);
            return lastDayOfMonth;
        }
        public static bool IsFirstDayOfMonth(this DateTime dateTime)
        {
            DateTime firstDayOfMonth = new(dateTime.Year, dateTime.Month, 1);
            return dateTime.Date == firstDayOfMonth;
        }
        public static DateTime Add(this DateTime datetime, TimeSpan value)
        {
            if (value.Ticks > 0)
                return DateTime.MaxValue - datetime >= value ? DateTime.MaxValue : datetime.Add(value);
            return datetime;
        }
        public static DateTime RoundMinute(this DateTime datetime, int accumulator)
        {
            var result = datetime.Round(TimeSpan.FromMinutes(accumulator));
            if (result < datetime)
                result = result.AddMinutes(accumulator);
            return result;
        }
        public static int GetDayOfWeekNumber(this DayOfWeek dayOfWeek)
        {
            var result = 0;

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    result = 1;
                    break;

                case DayOfWeek.Tuesday:
                    result = 2;
                    break;

                case DayOfWeek.Wednesday:
                    result = 3;
                    break;

                case DayOfWeek.Thursday:
                    result = 4;
                    break;

                case DayOfWeek.Friday:
                    result = 5;
                    break;

                case DayOfWeek.Saturday:
                    result = 6;
                    break;

                case DayOfWeek.Sunday:
                    result = 7;
                    break;
            }

            return result;
        }


    }
}
