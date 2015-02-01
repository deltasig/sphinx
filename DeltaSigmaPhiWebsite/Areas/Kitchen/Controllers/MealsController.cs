namespace DeltaSigmaPhiWebsite.Areas.Kitchen.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, House Steward")]
    public class MealsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.Meals.ToListAsync());
        }

        [Authorize(Roles = "Alumnus, Active, Neophyte, Pledge")]
        public async Task<ActionResult> Schedule(int week = 0)
        {
            ViewBag.week = week;



            return View();
        }

        public async Task<ActionResult> Create()
        {
            var model = new CreateMealModel
            {
                MealItems = await base.GetMealItemsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateMealModel model)
        {
            if (!ModelState.IsValid || model.SelectedMealItemIds == null) return RedirectToAction("Create");

            var meal = new Meal();
            _db.Meals.Add(meal);
            await _db.SaveChangesAsync();
            foreach(var id in model.SelectedMealItemIds)
            {
                var item = new MealToItem
                {
                    MealId = meal.MealId, 
                    MealItemId = id
                };
                _db.MealToItems.Add(item);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var meal = await _db.Meals.FindAsync(id);
            if (meal == null)
            {
                return HttpNotFound();
            }

            var model = new EditMealModel
            {
                MealItems = await base.GetMealItemsSelectListAsync(), 
                SelectedMealItemIds = meal.MealsToItems.Select(m => m.MealItemId).ToArray(),
                MealId = meal.MealId
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditMealModel model)
        {
            if (!ModelState.IsValid || model.SelectedMealItemIds == null) return RedirectToAction("Edit", new { id = model.MealId });

            var meal = await _db.Meals.FindAsync(model.MealId);
            if (meal == null)
            {
                return HttpNotFound();
            }

            foreach (var i in meal.MealsToItems.ToList())
            {
                if (!model.SelectedMealItemIds.Contains(i.MealItemId))
                {
                    _db.Entry(i).State = EntityState.Deleted;
                }
            }
            var remainingIds = meal.MealsToItems.Select(m => m.MealItemId).ToList();
            foreach (var i in model.SelectedMealItemIds)
            {
                if (remainingIds.Contains(i)) continue;
                var mealItem = new MealToItem
                {
                    MealId = meal.MealId, 
                    MealItemId = i
                };
                meal.MealsToItems.Add(mealItem);
            }

            _db.Entry(meal).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var meal = await _db.Meals.FindAsync(id);
            if (meal == null)
            {
                return HttpNotFound();
            }
            return View(meal);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var meal = await _db.Meals.FindAsync(id);
            _db.Meals.Remove(meal);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
