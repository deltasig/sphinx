namespace DeltaSigmaPhiWebsite
{
    using System;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using System.Threading.Tasks;
    using App_Start;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using FluentScheduler;
    using FluentScheduler.Model;
    using WebMatrix.WebData;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["MigrateDatabaseToLatestVersion"]))
            {
                var migrator = new DbMigrator(new Migrations.Configuration());
                migrator.Update();
            }
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

            TaskManager.UnobservedTaskException += TaskManager_UnobservedTaskException;
            TaskManager.Initialize(new DspTaskRegistry());
        }

        static void TaskManager_UnobservedTaskException(TaskExceptionInformation info, UnhandledExceptionEventArgs e)
        {
            
        }
    }
}