namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
    public class SoberTypesController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.SoberTypes.Include(m => m.Signups).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SoberType soberType)
        {
            if (!ModelState.IsValid) return View(soberType);

            _db.SoberTypes.Add(soberType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }
            return View(soberType);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SoberType soberType)
        {
            if (!ModelState.IsValid) return View(soberType);

            _db.Entry(soberType).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }
            return View(soberType);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var soberType = await _db.SoberTypes.FindAsync(id);
            _db.SoberTypes.Remove(soberType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Alumnus, Active, Pledge, Neophyte")]
        public async Task<ActionResult> Info(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }
            return View(soberType);
        }
    }
}
