namespace Dsp.Areas.Kitchen.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, House Steward")]
    public class MealItemTypesController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.MealItemTypes.Include(m => m.MealItems).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MealItemType mealItemType)
        {
            if (!ModelState.IsValid) return View(mealItemType);

            _db.MealItemTypes.Add(mealItemType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealItemType = await _db.MealItemTypes.FindAsync(id);
            if (mealItemType == null)
            {
                return HttpNotFound();
            }
            return View(mealItemType);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MealItemType mealItemType)
        {
            if (!ModelState.IsValid) return View(mealItemType);

            _db.Entry(mealItemType).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealItemType = await _db.MealItemTypes.FindAsync(id);
            if (mealItemType == null)
            {
                return HttpNotFound();
            }
            return View(mealItemType);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mealItemType = await _db.MealItemTypes.FindAsync(id);
            _db.MealItemTypes.Remove(mealItemType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
