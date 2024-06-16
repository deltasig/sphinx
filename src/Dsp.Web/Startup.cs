using Microsoft.Owin;
using Owin;
using RazorEngine;
using RazorEngine.Templating;
using System.IO;
using System.Net;

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

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }
}
