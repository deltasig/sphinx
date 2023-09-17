using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Dsp.WebCore.Extensions
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 6,
                RequiredUniqueChars = 1,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(RandomNumberGenerator.GetInt32(chars.Count),
                    randomChars[0][RandomNumberGenerator.GetInt32(randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(RandomNumberGenerator.GetInt32(chars.Count),
                    randomChars[1][RandomNumberGenerator.GetInt32(randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(RandomNumberGenerator.GetInt32(chars.Count),
                    randomChars[2][RandomNumberGenerator.GetInt32(randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(RandomNumberGenerator.GetInt32(chars.Count),
                    randomChars[3][RandomNumberGenerator.GetInt32(randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[RandomNumberGenerator.GetInt32(randomChars.Length)];
                chars.Insert(RandomNumberGenerator.GetInt32(chars.Count),
                    rcs[RandomNumberGenerator.GetInt32(rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
