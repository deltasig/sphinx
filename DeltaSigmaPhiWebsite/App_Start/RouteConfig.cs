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

            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "StudyHours",
                url: "StudyHours/{action}/{id}",
                defaults: new { controller = "StudyHours", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Incidents",
                url: "Incidents/{action}/{id}",
                defaults: new { controller = "Incidents", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Classes",
                url: "Classes/{action}/{id}",
                defaults: new { controller = "Classes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ServiceHours",
                url: "ServiceHours/{action}/{id}",
                defaults: new { controller = "ServiceHours", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Events",
                url: "Events/{action}/{id}",
                defaults: new { controller = "Events", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PhoneNumbers",
                url: "PhoneNumbers/{action}/{id}",
                defaults: new { controller = "PhoneNumbers", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Addresses",
                url: "Addresses/{action}/{id}",
                defaults: new { controller = "Addresses", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Semesters",
                url: "Semesters/{action}/{id}",
                defaults: new { controller = "Semesters", id = UrlParameter.Optional }
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
                name: "ErrorIndex",
                url: "Error/",
                defaults: new { controller = "Error", action = "NotFound"}
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