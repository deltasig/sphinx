namespace Dsp.Extensions
{
    using Data;
    using Microsoft.AspNet.Identity;
    using System.Security.Principal;

    public static class UserExtensions
    {
        public static string GetAvatar(this IIdentity user)
        {
            var path = string.Empty;
            if (user.IsAuthenticated)
            {
                using (var db = new SphinxDbContext())
                {
                    var member = db.Users.Find(user.GetUserId<int>());
                    path = member.AvatarPath;
                }
            }
            return path;
        }
    }
}