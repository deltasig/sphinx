using System.Web.Mvc;

namespace Dsp.Areas.House
{
    public class HouseAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "House";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "House_default",
                "House/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}