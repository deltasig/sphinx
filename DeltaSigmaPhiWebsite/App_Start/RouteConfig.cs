namespace DeltaSigmaPhiWebsite.App_Start
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AccountIndex",
                url: "Account/",
                defaults: new { controller = "Account", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{id}",
                defaults: new { controller = "Account", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SphinxIndex",
                url: "Sphinx/",
                defaults: new { controller = "Sphinx", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Sphinx",
                url: "Sphinx/{action}/{id}",
                defaults: new { controller = "Sphinx", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Root",
                url: "{action}/{id}",
                defaults: new { controller = "Home", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default", // Route name
                url: "{controller}/{action}/{id}", // URL with parameters
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }
    }
}