namespace Dsp.Web.Areas.Scholarships.Controllers
{
    using Dsp.Web.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public class TypesController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];
            return View(await _db.ScholarshipTypes.ToListAsync());
        }
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ScholarshipType model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.ScholarshipTypes.Add(model);
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Scholarship Type created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scholarshiptype = await _db.ScholarshipTypes.FindAsync(id);
            if (scholarshiptype == null)
            {
                return HttpNotFound();
            }
            return View(scholarshiptype);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ScholarshipType model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Scholarship Type updated successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scholarshiptype = await _db.ScholarshipTypes.FindAsync(id);
            if (scholarshiptype == null || scholarshiptype.Applications.Any())
            {
                return HttpNotFound();
            }
            return View(scholarshiptype);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var scholarshipType = await _db.ScholarshipTypes.FindAsync(id);
            if (scholarshipType.Applications.Any())
            {
                TempData[FailureMessageKey] = "The " + scholarshipType.Name +
                    " Scholarship Type could not be deleted because it has existing applications associated with it.";
                return RedirectToAction("Index");
            }

            _db.ScholarshipTypes.Remove(scholarshipType);
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Scholarship Type deleted successfully.";
            return RedirectToAction("Index");
        }

    }
}
