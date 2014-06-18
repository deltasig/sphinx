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
                name: "PhoneNumber",
                url: "PhoneNumber/{action}/{id}",
                defaults: new { controller = "PhoneNumber", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Address",
                url: "Address/{action}/{id}",
                defaults: new { controller = "Address", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Semester",
                url: "Semester/{action}/{id}",
                defaults: new { controller = "Semester", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AccountActions",
                url: "Account/{action}/{model}",
                defaults: new { controller = "Account", model = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AccountIndex",
                url: "Account/{userName}",
                defaults: new { controller = "Account", action = "Index", userName = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{userName}",
                defaults: new { controller = "Account", userName = UrlParameter.Optional }
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
                name: "Errors",
                url: "Error/{action}",
                defaults: new { controller = "Error" }
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