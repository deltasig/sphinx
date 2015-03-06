namespace DeltaSigmaPhiWebsite.App_Start
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("elmah.axd");
            routes.IgnoreRoute("robots.txt");
            routes.IgnoreRoute("sitemap.xml");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                "ErrorIndex",
                "Error/",
                new { controller = "Error", action = "NotFound" }
            );

            routes.MapRoute(
                "Errors",
                "Error/{action}",
                new { controller = "Error" }
            );

            routes.MapRoute(
                "Root",
                "{action}/{id}",
                new { controller = "Home", id = UrlParameter.Optional },
                new [] { "DeltaSigmaPhiWebsite.Controllers" }
            );

            routes.MapRoute(
                 "Default", 
                 "{controller}/{action}/{id}",
                 new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "DeltaSigmaPhiWebsite.Controllers" }
            );
        }
    }
}