namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Web.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, House Steward")]
    public class MealPeriodsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = await _db.MealPeriods.ToListAsync();
            foreach (var m in model)
            {
                m.StartTime = ConvertUtcToCst(m.StartTime);
                m.EndTime = ConvertUtcToCst(m.EndTime);
            }
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MealPeriod mealperiod)
        {
            if (!ModelState.IsValid) return View(mealperiod);

            mealperiod.StartTime = ConvertCstToUtc(mealperiod.StartTime);
            mealperiod.EndTime = ConvertCstToUtc(mealperiod.EndTime);
            _db.MealPeriods.Add(mealperiod);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealperiod = await _db.MealPeriods.FindAsync(id);
            if (mealperiod == null)
            {
                return HttpNotFound();
            }
            mealperiod.StartTime = ConvertUtcToCst(mealperiod.StartTime);
            mealperiod.EndTime = ConvertUtcToCst(mealperiod.EndTime);
            return View(mealperiod);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MealPeriod mealperiod)
        {
            if (!ModelState.IsValid) return View(mealperiod);

            mealperiod.StartTime = ConvertCstToUtc(mealperiod.StartTime);
            mealperiod.EndTime = ConvertCstToUtc(mealperiod.EndTime);
            _db.Entry(mealperiod).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mealperiod = await _db.MealPeriods.FindAsync(id);
            if (mealperiod == null)
            {
                return HttpNotFound();
            }
            mealperiod.StartTime = ConvertUtcToCst(mealperiod.StartTime);
            mealperiod.EndTime = ConvertUtcToCst(mealperiod.EndTime);
            return View(mealperiod);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mealperiod = await _db.MealPeriods.FindAsync(id);
            _db.MealPeriods.Remove(mealperiod);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
