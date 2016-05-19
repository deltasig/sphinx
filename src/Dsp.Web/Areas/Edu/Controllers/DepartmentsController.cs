namespace Dsp.Web.Areas.Edu.Controllers
{
    using Dsp.Web.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class DepartmentsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(await _db.Departments.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.Departments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Create(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Departments.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was added successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.Departments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.Departments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.Departments.FindAsync(id);
            _db.Departments.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
