namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using System.Linq;
    using System.Web.Mvc;

    [AllowAnonymous]
    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws)
        {
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contacts()
        {
            var currentSemester = GetThisSemester();

            if (currentSemester == null) return View();

            var model = uow.LeaderRepository.SelectAll().Where(l => l.SemesterId == currentSemester.SemesterId).ToList();

            return View(model);
        }

        public ActionResult Recruitment()
        {
            return View();
        }

        public ActionResult Scholarships()
        {
            return View();
        }
        public ActionResult Academics()
        {
            return View();
        }

    }
}
