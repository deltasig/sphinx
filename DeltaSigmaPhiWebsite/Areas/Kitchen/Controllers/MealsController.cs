using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeltaSigmaPhiWebsite.Entities;
using DeltaSigmaPhiWebsite.Controllers;
using DeltaSigmaPhiWebsite.Areas.Kitchen.Models;

namespace DeltaSigmaPhiWebsite.Areas.Meals.Controllers
{
    public class MealsController : BaseController
    {

        // GET: Meals/Meals
        public async Task<ActionResult> Index()
        {
            return View(await _db.Meals.ToListAsync());
        }

        // GET: Meals/Meals/Create
        public async Task<ActionResult> Create()
        {
            var model = new CreateMealModel();
            model.MealItems = await base.GetMealItemsSelectListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateMealModel model)
        {
            if (ModelState.IsValid && model.SelectedMealItemIds.Any())
            {
                var meal = new Meal();
                _db.Meals.Add(meal);
                await _db.SaveChangesAsync();
                foreach(var id in model.SelectedMealItemIds)
                {
                    var item = new MealToItem();
                    item.MealId = meal.MealId;
                    item.MealItemId = id;
                    _db.MealToItems.Add(item);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Create");
        }

        // GET: Meals/Meals/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Meal meal = await _db.Meals.FindAsync(id);
            if (meal == null)
            {
                return HttpNotFound();
            }
            return View(meal);
        }

        // POST: Meals/Meals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MealId")] Meal meal)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(meal).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(meal);
        }

        // GET: Meals/Meals/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Meal meal = await _db.Meals.FindAsync(id);
            if (meal == null)
            {
                return HttpNotFound();
            }
            return View(meal);
        }

        // POST: Meals/Meals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Meal meal = await _db.Meals.FindAsync(id);
            _db.Meals.Remove(meal);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
