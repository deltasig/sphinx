namespace DeltaSigmaPhiWebsite.App_Start
{
    using Microsoft.Web.WebPages.OAuth;

    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            //OAuthWebSecurity.RegisterMicrosoftClient("", "");
            //OAuthWebSecurity.RegisterTwitterClient("", "");
            OAuthWebSecurity.RegisterFacebookClient("648128831898394", "301287924d5874aa9f52d65c268311fb");
            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
