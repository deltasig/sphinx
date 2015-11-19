namespace Dsp.Areas.Alumni.Controllers
{
    using Dsp.Controllers;
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