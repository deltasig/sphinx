using System.Web.WebPages;

namespace DeltaSigmaPhiWebsite.Controllers
{
    using Entities;
    using Models;
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

            var model = await _db.Leaders
                .Where(l => l.SemesterId == currentSemester.SemesterId)
                .ToListAsync();

            return View(model);
        }

        public async Task<ActionResult> Recruitment()
        {
            return View(await _db.ScholarshipApps
                .Where(s => s.IsPublic && s.Type.Name == "Building Better Men Scholarship").ToListAsync());
        }

        public async Task<ActionResult> Scholarships(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var model = new ExternalScholarshipModel
            {
                Applications = await _db.ScholarshipApps.Where(s => s.IsPublic).ToListAsync(),
                Types = await _db.ScholarshipTypes.ToListAsync()
            };

            return View(model);
        }

        public ActionResult Check()
        {
            return Content("OK");
        }
    }
}
