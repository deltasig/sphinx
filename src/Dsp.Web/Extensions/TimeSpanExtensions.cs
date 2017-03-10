namespace Dsp.Web.Extensions
{
    using System;
    using System.Text;

    public static class TimeSpanExtensions
    {
        public static string ToUserFriendlyString(this TimeSpan timeSpan)
        {
            var output = new StringBuilder();

            if (timeSpan.TotalMinutes < 1)
            {
                output.Append("just now");
            }
            else
            {
                var value = 0;
                if (timeSpan.TotalMinutes < 60)
                {
                    output.Append($"{(int)timeSpan.TotalMinutes} minute{(value != 1 ? "s" : string.Empty)} ago");
                }
                else if (timeSpan.TotalMinutes <= 60)
                {
                    output.Append("an hour ago");
                }
                else if (timeSpan.TotalHours < 24)
                {
                    output.Append($"{(int)timeSpan.TotalHours} hours ago");
                }
                else if (timeSpan.TotalDays < 2)
                {
                    output.Append("yesterday");
                }
                else if (timeSpan.TotalDays < 7)
                {
                    output.Append($"{(int)timeSpan.TotalDays} days ago");
                }
                else if (timeSpan.TotalDays < 14)
                {
                    output.Append("a week ago");
                }
                else if (timeSpan.TotalDays < 31)
                {
                    output.Append($"{(int)timeSpan.TotalDays / 7} weeks ago");
                }
                else if (timeSpan.TotalDays < 62)
                {
                    output.Append("a month ago");
                }
                else if (timeSpan.TotalDays < 365)
                {
                    output.Append($"{(int)timeSpan.TotalDays / 31} months ago");
                }
                else if (timeSpan.TotalDays < 730)
                {
                    output.Append("a year ago");
                }
                else
                {
                    output.Append($"{(int)timeSpan.TotalDays / 365} years ago");
                }
            }

            return output.ToString();
        }

        public static string ToPreciseTimeUntilString(this TimeSpan timeSpan)
        {
            var output = new StringBuilder();

            if (timeSpan.TotalDays >= 1)
            {
                output.Append($"{(int)timeSpan.TotalDays} day{(Math.Abs(timeSpan.TotalSeconds - 1) > double.Epsilon ? "s" : string.Empty)}");
            }
            else if (timeSpan.TotalHours >= 1)
            {
                output.Append($"{(int)timeSpan.TotalHours} hour{(Math.Abs(timeSpan.TotalSeconds - 1) > double.Epsilon ? "s" : string.Empty)}");
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                output.Append($"{(int)timeSpan.TotalMinutes} minute{(Math.Abs(timeSpan.TotalSeconds - 1) > double.Epsilon ? "s" : string.Empty)}");
            }
            else if (timeSpan.TotalSeconds >= 0)
            {
                output.Append($"{(int)timeSpan.TotalSeconds} second{(Math.Abs(timeSpan.TotalSeconds - 1) > double.Epsilon ? "s" : string.Empty)}");
            }
            else
            {
                output.Append("An inexplicable amount of time.");
            }

            return output.ToString();
        }
    }
}