using System.Web.Mvc;

namespace DeltaSigmaPhiWebsite.Areas.Alumni
{
    public class AlumniAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Alumni";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Alumni_default",
                "Alumni/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}