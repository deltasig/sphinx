namespace DeltaSigmaPhiWebsite.Controllers
{
    using Models.Entities;
    using Models.ViewModels;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, President, Secretary, Academics")]
    public class SemestersController : BaseController
    {
        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Index()
        {
            return View(_db.Semesters.OrderByDescending(s => s.DateStart).ToList());
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
        public ActionResult Create(CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Semesters.Add(model.Semester);
            _db.SaveChanges();
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = _db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            _db.Entry(semester).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = _db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var semester = _db.Semesters.Find(id);
            _db.Semesters.Remove(semester);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
