using Dsp.Services;
using Dsp.Web.Extensions;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Web.Configuration;

[assembly: OwinStartupAttribute(typeof(Dsp.Web.Startup))]
namespace Dsp.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Emails/SoberSchedule.cshtml");
            var template = File.ReadAllText(path);
            Engine.Razor.AddTemplate("sse", template);

            var options = new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(60)
            };
            var dbConnectionString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(dbConnectionString, options);

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            RecurringJob.AddOrUpdate("sober-schedule-email", () => EmailService.TryToSendSoberSchedule(), Cron.Daily(16), tz);
        }
    }
}
