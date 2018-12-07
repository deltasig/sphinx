namespace Dsp.Web.Areas.Alumni.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
    public class LeadersController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var currentSemester = await GetThisSemesterAsync();

            if (currentSemester == null) return View();

            var model = await _db.Leaders
                .Where(l =>
                    l.SemesterId == currentSemester.SemesterId &&
                    l.Position.Type == Position.PositionType.Alumni)
                .ToListAsync();

            return View(model);
        }
    }
}