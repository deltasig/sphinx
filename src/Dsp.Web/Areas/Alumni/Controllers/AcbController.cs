﻿namespace Dsp.Web.Areas.Alumni.Controllers
{
    using Dsp.Web.Controllers;
    using System.Web.Mvc;

    [AllowAnonymous]
    public class AcbController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}