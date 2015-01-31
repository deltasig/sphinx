using System.Web.Mvc;

namespace DeltaSigmaPhiWebsite.Areas.Meals
{
    public class KitchenAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Kitchen";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Kitchen_default",
                "Kitchen/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}