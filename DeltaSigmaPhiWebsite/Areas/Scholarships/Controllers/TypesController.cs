namespace DeltaSigmaPhiWebsite.Areas.Scholarships.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
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
            return View(await _db.ScholarshipTypes.ToListAsync());
        }
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ScholarshipType scholarshiptype)
        {
            if (!ModelState.IsValid) return View(scholarshiptype);

            _db.ScholarshipTypes.Add(scholarshiptype);
            await _db.SaveChangesAsync();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ScholarshipType scholarshiptype)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(scholarshiptype).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(scholarshiptype);
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var scholarshiptype = await _db.ScholarshipTypes.FindAsync(id);
            if (scholarshiptype.Applications.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _db.ScholarshipTypes.Remove(scholarshiptype);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
