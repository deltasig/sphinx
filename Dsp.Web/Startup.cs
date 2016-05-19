using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Dsp.Web.Startup))]
namespace Dsp.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
