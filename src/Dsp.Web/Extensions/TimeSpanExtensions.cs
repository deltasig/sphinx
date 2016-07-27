namespace Dsp.Web.Extensions
{
    using System;
    using System.Text;

    public static class TimeSpanExtensions
    {
        public static string ToUserFriendlyString(this TimeSpan timeSpan)
        {
            var value = 0;
            var output = new StringBuilder();

            if(timeSpan.TotalMinutes < 1)
            {
                output.Append("just now");
            }
            else
            {
                if (timeSpan.TotalMinutes < 60)
                {
                    value = (int)timeSpan.TotalMinutes;
                    output.Append(value);
                    output.Append(" minute");
                }
                else if (timeSpan.TotalHours < 24)
                {
                    value = (int)timeSpan.TotalHours;
                    output.Append(value);
                    output.Append(" hour");
                }
                else if (timeSpan.TotalDays < 7)
                {
                    value = (int)timeSpan.TotalDays;
                    output.Append(value);
                    output.Append(" day");
                }
                else if (timeSpan.TotalDays < 31)
                {
                    value = (int)timeSpan.TotalDays / 7;
                    output.Append(value);
                    output.Append(" week");
                }
                else if (timeSpan.TotalDays < 365)
                {
                    value = (int)timeSpan.TotalDays / 31;
                    output.Append(value);
                    output.Append(" month");
                }
                else
                {
                    value = (int)timeSpan.TotalDays / 365;
                    output.Append(value);
                    output.Append(" year");
                }
                if (value != 1)
                    output.Append("s");
                output.Append(" ago");
            }

            return output.ToString();
        }

        public static string ToPreciseTimeUntilString(this TimeSpan timeSpan)
        {
            var output = new StringBuilder();

            if (timeSpan.TotalDays >= 1)
            {
                output.Append(string.Format("{0} day", (int)timeSpan.TotalDays));
                output.Append(timeSpan.TotalDays != 1 ? "s" : string.Empty);
            }
            else if(timeSpan.TotalHours >= 1)
            {
                output.Append(string.Format("{0} hour", (int)timeSpan.TotalHours));
                output.Append(timeSpan.TotalHours != 1 ? "s" : string.Empty);
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                output.Append(string.Format("{0} minute", (int)timeSpan.TotalMinutes));
                output.Append(timeSpan.TotalMinutes != 1 ? "s" : string.Empty);
            }
            else if (timeSpan.TotalSeconds >= 0)
            {
                output.Append(string.Format("{0} second", (int)timeSpan.TotalSeconds));
                output.Append(timeSpan.TotalSeconds != 1 ? "s" : string.Empty);
            }
            else
            {
                output.Append("An inexplicable amount of time.");
            }

            return output.ToString();
        }
    }
}