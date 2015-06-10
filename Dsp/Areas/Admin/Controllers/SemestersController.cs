namespace Dsp.Areas.Admin.Controllers
{
    using Entities;
    using global::Dsp.Controllers;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, President, Secretary, Academics, Service")]
    public class SemestersController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _db.Semesters.OrderByDescending(s => s.DateStart).ToListAsync());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Semester.DateStart = base.ConvertCstToUtc(model.Semester.DateStart);
            model.Semester.DateEnd = base.ConvertCstToUtc(model.Semester.DateEnd);
            model.Semester.TransitionDate = base.ConvertCstToUtc(model.Semester.TransitionDate);
            _db.Semesters.Add(model.Semester);
            await _db.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            semester.DateStart = base.ConvertUtcToCst(semester.DateStart);
            semester.DateEnd = base.ConvertUtcToCst(semester.DateEnd);
            semester.TransitionDate = base.ConvertUtcToCst(semester.TransitionDate);
            return View(semester);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            semester.DateStart = base.ConvertCstToUtc(semester.DateStart);
            semester.DateEnd = base.ConvertCstToUtc(semester.DateEnd);
            semester.TransitionDate = base.ConvertCstToUtc(semester.TransitionDate);
            _db.Entry(semester).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            _db.Semesters.Remove(semester);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
