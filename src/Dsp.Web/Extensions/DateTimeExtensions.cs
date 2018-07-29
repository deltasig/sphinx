namespace Dsp.Web.Extensions
{
    using System;
    using System.Globalization;

    public static class DateTimeExtensions
    {
        public static int GetWeekOfYear(DateTime time)
        {
            return CultureInfo.GetCultureInfo("en-US").Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear, CultureInfo ci)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            var firstWeekDay = jan1.AddDays(daysOffset);
            var firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if (firstWeek <= 1 || firstWeek > 50)
            {
                weekOfYear -= 1;
            }
            return firstWeekDay.AddDays(weekOfYear * 7);
        }

        public static DateTime LastDateOfWeek(int year, int weekOfYear, CultureInfo ci)
        {
            return FirstDateOfWeek(year, weekOfYear, ci).AddDays(6);
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-4);
        }

        public static DateTime FirstDateOfWeek(DateTime dateTime)
        {
            var weekOfYear = GetWeekOfYear(dateTime);
            return FirstDateOfWeek(dateTime.Year, weekOfYear, CultureInfo.GetCultureInfo("en-US"));
        }

        public static string GetDaySuffix(this DateTime date)
        {
            switch (date.Day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static DateTime FromUtcToCst(this DateTime utc)
        {
            return ConvertFromUtc(utc, "Central Standard Time");
        }

        public static DateTime FromCstToUtc(this DateTime cst)
        {
            return ConvertToUtc(cst, "Central Standard Time");
        }

        public static DateTime ConvertToUtc(DateTime cst, string timezoneString)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneString);
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }

        public static DateTime ConvertFromUtc(DateTime utc, string timezoneString)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneString);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
    }
}