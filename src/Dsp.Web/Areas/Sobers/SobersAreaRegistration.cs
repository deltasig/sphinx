using System.Web.Mvc;

namespace Dsp.Web.Areas.Sobers
{
    public class SobersAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Sobers";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Sobers_default",
                "Sobers/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Dsp.Web.Areas.Sobers.Controllers" }
            );
        }
    }
}