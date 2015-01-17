using System.Web.Mvc;

namespace DeltaSigmaPhiWebsite.Areas.Scholarships
{
    public class ScholarshipsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Scholarships";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Scholarships_default",
                "Scholarships/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}