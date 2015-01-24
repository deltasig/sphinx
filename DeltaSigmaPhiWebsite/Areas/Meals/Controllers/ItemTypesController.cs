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
    public class ItemTypesController : Controller
    {
        private DspDbContext db = new DspDbContext();

        // GET: Meals/ItemTypes
        public async Task<ActionResult> Index()
        {
            return View(await db.MealItemTypes.Include(m => m.MealItems).ToListAsync());
        }

        // GET: Meals/ItemTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Meals/ItemTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MealItemTypeId,Name")] MealItemType mealItemType)
        {
            if (ModelState.IsValid)
            {
                db.MealItemTypes.Add(mealItemType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(mealItemType);
        }

        // GET: Meals/ItemTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealItemType mealItemType = await db.MealItemTypes.FindAsync(id);
            if (mealItemType == null)
            {
                return HttpNotFound();
            }
            return View(mealItemType);
        }

        // POST: Meals/ItemTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MealItemTypeId,Name")] MealItemType mealItemType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mealItemType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(mealItemType);
        }

        // GET: Meals/ItemTypes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MealItemType mealItemType = await db.MealItemTypes.FindAsync(id);
            if (mealItemType == null)
            {
                return HttpNotFound();
            }
            return View(mealItemType);
        }

        // POST: Meals/ItemTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            MealItemType mealItemType = await db.MealItemTypes.FindAsync(id);
            db.MealItemTypes.Remove(mealItemType);
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
