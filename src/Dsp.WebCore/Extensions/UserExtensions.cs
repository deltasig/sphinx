namespace Dsp.WebCore.Extensions;

using Dsp.Data;
using System.Security.Claims;
using System.Security.Principal;

public static class UserExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        string userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Convert.ToInt32(userId);
    }

    public static string GetUserName(this ClaimsPrincipal principal)
    {
        string userId = principal.FindFirstValue(ClaimTypes.Name);
        return userId;
    }
}
