namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using System.Web.Mvc;

    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws)
            : base(uow, ws, oaws)
        {
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult HowToJoin()
        {
            return View();
        }

        public ActionResult BuildingBetterMenScholarship()
        {
            return View();
        }
        public ActionResult Academics()
        {
            return View();
        }

        public ActionResult Officers()
        {
            return View();
        }
        public ActionResult Chairmen()
        {
            return View();
        }
    }
}
