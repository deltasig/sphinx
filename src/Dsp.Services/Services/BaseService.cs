using Dsp.Services.Interfaces;
using System;
using System.Globalization;

namespace Dsp.Services
{
    public abstract class BaseService : IService
    {
        public BaseService()
        {

        }

        public virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }

        public virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }

        protected virtual string ToTitleCaseString(string original)
        {
            var formattedText = string.Empty;

            var words = original.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                if (!IsAllUpper(words[i]) && !Char.IsNumber(words[i][0]))
                {
                    words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant());
                }
                formattedText += words[i];
                if (i < words.Length - 1)
                    formattedText += " ";
            }

            return formattedText;
        }

        private bool IsAllUpper(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }
    }
}
