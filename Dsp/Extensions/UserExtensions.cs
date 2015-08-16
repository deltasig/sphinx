namespace Dsp.Extensions
{
    using Data;
    using Microsoft.AspNet.Identity;
    using System.Security.Principal;
    using Areas.Members.Controllers;

    public static class UserExtensions
    {
        public static string GetAvatar(this IIdentity user)
        {
            var imageName = string.Empty;
            if (user.IsAuthenticated)
            {
                using (var db = new SphinxDbContext())
                {
                    var member = db.Users.Find(user.GetUserId<int>());
                    imageName = member.AvatarPath;
                }
            }
            var filePath = AccountController.ImageUpload.GetUploadPath(imageName);
            var fileExists = System.IO.File.Exists(filePath);
            return fileExists ? imageName : "NoAvatar.jpg";
        }
    }
}