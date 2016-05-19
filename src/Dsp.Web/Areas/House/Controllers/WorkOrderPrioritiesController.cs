namespace Dsp.Web.Areas.House.Controllers
{
    using Dsp.Web.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, House Manager")]
    public class WorkOrderPrioritiesController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(await _db.WorkOrderPriorities.Include(w => w.PriorityChanges).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(WorkOrderPriority model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.WorkOrderPriorities.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.WorkOrderPriorities.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(WorkOrderPriority model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " priority updated successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.WorkOrderPriorities.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.WorkOrderPriorities.FindAsync(id);
            _db.WorkOrderPriorities.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " priority deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}