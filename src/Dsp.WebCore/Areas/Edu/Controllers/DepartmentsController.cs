namespace Dsp.WebCore.Areas.Edu.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.WebCore.Controllers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Net;
    using System.Threading.Tasks;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator")]
    public class DepartmentsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(await Context.Departments.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var model = await Context.Departments.FindAsync(id);
            if (model == null)
            {
                return NotFound();
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

            Context.Departments.Add(model);
            await Context.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was added successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var model = await Context.Departments.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);

            Context.Entry(model).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var model = await Context.Departments.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await Context.Departments.FindAsync(id);
            Context.Departments.Remove(model);
            await Context.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Name + " department was deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
