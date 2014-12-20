namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [AllowAnonymous]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Contacts()
        {
            var currentSemester = await GetThisSemesterAsync();

            if (currentSemester == null) return View();

            var model = await _db.Leaders.Where(l => l.SemesterId == currentSemester.SemesterId).ToListAsync();

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
