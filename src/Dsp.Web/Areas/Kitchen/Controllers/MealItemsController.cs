namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Alumnus, Active, Neophyte, Pledge")]
    public class MealItemsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.ReturnUrl = "MealItems/Index";
            var mealItems = await _db.MealItems.ToListAsync();
            return View(mealItems);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MealItem mealItem)
        {
            if (!ModelState.IsValid) return RedirectToAction("Create");

            mealItem.Name = base.ToTitleCaseString(mealItem.Name);
            mealItem.Name = mealItem.Name.Replace("And", "&");
            _db.MealItems.Add(mealItem);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealItem = await _db.MealItems.FindAsync(id);
            if (mealItem == null)
            {
                return HttpNotFound();
            }

            return View(mealItem);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Edit(MealItem mealItem)
        {
            if (!ModelState.IsValid) return RedirectToAction("Edit", new { id = mealItem.Id });

            mealItem.Name = base.ToTitleCaseString(mealItem.Name);
            mealItem.Name = mealItem.Name.Replace("And", "&");
            _db.Entry(mealItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealItem = await _db.MealItems.FindAsync(id);
            if (mealItem == null)
            {
                return HttpNotFound();
            }
            return View(mealItem);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mealItem = await _db.MealItems.FindAsync(id);
            _db.MealItems.Remove(mealItem);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
