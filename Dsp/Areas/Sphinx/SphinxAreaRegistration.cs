using System.Web.Mvc;

namespace Dsp.Areas.Sphinx
{
    public class SphinxAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Sphinx";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Sphinx_default",
                "Sphinx/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Dsp.Areas.Sphinx.Controllers" }
            );
        }
    }
}