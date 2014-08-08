namespace DeltaSigmaPhiWebsite.App_Start
{
    using Microsoft.Web.WebPages.OAuth;

    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            //OAuthWebSecurity.RegisterMicrosoftClient("", "");
            //OAuthWebSecurity.RegisterTwitterClient("", "");
            OAuthWebSecurity.RegisterFacebookClient("312694035579415", "6b647683da7c13f846b5f38042f83b1f");
            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
