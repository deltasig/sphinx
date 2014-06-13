namespace DeltaSigmaPhiWebsite
{
    using App_Start;
    using Migrations;
    using System.Data.Entity.Migrations;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using WebMatrix.WebData;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var migrator = new DbMigrator(new Configuration());
            migrator.Update();

            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "Members", "UserId", "UserName", true);
            }
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }
    }
}