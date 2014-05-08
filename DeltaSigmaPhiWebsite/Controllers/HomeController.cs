namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Web.Mvc;
    using Data.UnitOfWork;

    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork uow) : base(uow) { }

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
