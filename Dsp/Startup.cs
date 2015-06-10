using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Dsp.Startup))]
namespace Dsp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
