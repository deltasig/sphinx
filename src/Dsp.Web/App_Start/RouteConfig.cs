namespace Dsp.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("robots.txt");
            routes.IgnoreRoute("sitemap.xml");
            routes.IgnoreRoute("Error/403.html");
            routes.IgnoreRoute("Error/404.13.html");
            routes.IgnoreRoute("Error/404.html");
            routes.IgnoreRoute("Error/500.html");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                "Errors",
                "Error/{action}",
                new { controller = "Error" }
            );

            routes.MapRoute(
                "Root",
                "{action}/{id}",
                new { controller = "Home", id = UrlParameter.Optional },
                new [] { "Dsp.Web.Controllers" }
            );

            routes.MapRoute(
                 "Default", 
                 "{controller}/{action}/{id}",
                 new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new [] { "Dsp.Web.Controllers" }
            );
        }
    }
}