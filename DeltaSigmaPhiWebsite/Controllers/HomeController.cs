namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Linq;
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
            var model = uow.MemberRepository.GetAll().Where(m => m.webpages_Roles.Any()).ToList();
            return View(model);
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

    }
}
