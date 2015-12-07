namespace Dsp.Extensions
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
            else if(timeSpan.TotalMinutes < 60)
            {
                value = (int)timeSpan.TotalMinutes;
                output.Append(value);
                output.Append(" minute");
            }
            else if(timeSpan.TotalHours < 24)
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
            return output.ToString();
        }
    }
}