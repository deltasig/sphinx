namespace DeltaSigmaPhiWebsite.Areas.Admin.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, President, Secretary, Academics")]
    public class SemestersController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public async Task<ActionResult> Index()
        {
            return View(await _db.Semesters.OrderByDescending(s => s.DateStart).ToListAsync());
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Semesters.Add(model.Semester);
            await _db.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
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
            return View(semester);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

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
