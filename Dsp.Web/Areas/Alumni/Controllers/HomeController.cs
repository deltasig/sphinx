namespace Dsp.Web.Areas.Alumni.Controllers
{
    using Dsp.Web.Controllers;
    using System.Web.Mvc;

    [AllowAnonymous]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Acb()
        {
            return View();
        }
        
        public ActionResult Connect()
        {
            return View();
        }
    }
}