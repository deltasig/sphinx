﻿using System.Web.Mvc;

namespace Dsp.Web.Areas.Edu
{
    public class EduAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Edu";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Edu_default",
                "Edu/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}