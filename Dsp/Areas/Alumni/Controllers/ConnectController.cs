namespace Dsp.Areas.Alumni.Controllers
{
    using Dsp.Controllers;
    using System.Web.Mvc;

    [AllowAnonymous]
    public class ConnectController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}