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

namespace DeltaSigmaPhiWebsite.Areas.Meals.Controllers
{
    public class MealItemsController : Controller
    {
        private DspDbContext db = new DspDbContext();

        // GET: Meals/Items
        public async Task<ActionResult> Index()
        {
            var mealItems = db.MealItems.Include(m => m.MealItemType);
            return View(await mealItems.ToListAsync());
        }

        // GET: Meals/Items/Create
        public ActionResult Create()
        {
            ViewBag.MealItemTypeId = new SelectList(db.MealItemTypes, "MealItemTypeId", "Name");
            return View();
        }

        // POST: Meals/Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MealItemId,MealItemTypeId,Name,IsGlutenFree")] MealItem mealItem)
        {
            if (ModelState.IsValid)
            {
                db.MealItems.Add(mealItem);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MealItemTypeId = new SelectList(db.MealItemTypes, "MealItemTypeId", "Name", mealItem.MealItemTypeId);
            return View(mealItem);
        }

        // GET: Meals/Items/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealItem mealItem = await db.MealItems.FindAsync(id);
            if (mealItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.MealItemTypeId = new SelectList(db.MealItemTypes, "MealItemTypeId", "Name", mealItem.MealItemTypeId);
            return View(mealItem);
        }

        // POST: Meals/Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MealItemId,MealItemTypeId,Name,IsGlutenFree")] MealItem mealItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mealItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.MealItemTypeId = new SelectList(db.MealItemTypes, "MealItemTypeId", "Name", mealItem.MealItemTypeId);
            return View(mealItem);
        }

        // GET: Meals/Items/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealItem mealItem = await db.MealItems.FindAsync(id);
            if (mealItem == null)
            {
                return HttpNotFound();
            }
            return View(mealItem);
        }

        // POST: Meals/Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MealItem mealItem = await db.MealItems.FindAsync(id);
            db.MealItems.Remove(mealItem);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
