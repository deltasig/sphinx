namespace DeltaSigmaPhiWebsite.Areas.Kitchen.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Alumnus, Active, Neophyte, Pledge")]
    public class MealsController : BaseController
    {
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Index()
        {
            return View(await _db.Meals.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<ActionResult> Schedule(int week = 0)
        {
            ViewBag.week = week;
            var startOfWeekCst = base.GetStartOfCurrentWeek().AddDays(7 * week);
            var startOfWeekUtc = base.ConvertCstToUtc(startOfWeekCst);
            var startOfNextWeekUtc = base.ConvertCstToUtc(startOfWeekCst.AddDays(7));

            var model = new MealScheduleModel();
            model.StartOfWeek = startOfWeekCst;
            model.Meals = await _db.Meals.ToListAsync();
            model.MealPeriods = await _db.MealPeriods.ToListAsync();
            model.UsersVotes = Request.IsAuthenticated 
                ? (await _db.Members.FindAsync(WebSecurity.CurrentUserId)).MealVotes 
                : new List<MealVote>();
            model.LatePlates = await _db.MealLatePlates
                .Where(m => m.MealToPeriod.Date >= startOfWeekUtc.Date && m.MealToPeriod.Date < startOfNextWeekUtc.Date)
                .ToListAsync();

            var existingAssignments = await _db.MealToPeriods
                .Where(m => m.Date >= startOfWeekUtc.Date && m.Date < startOfNextWeekUtc.Date)
                .ToListAsync();

            var allSlots = new List<MealToPeriod>();
            foreach (var p in model.MealPeriods)
            {
                for (var i = 0; i < 7; i++)
                {
                    var slot = existingAssignments
                        .SingleOrDefault(m => 
                            m.MealPeriodId == p.MealPeriodId && 
                            m.Date == startOfWeekUtc.AddDays(i).Date);
                    if (slot == null)
                    {
                        slot = new MealToPeriod
                        {
                            MealPeriodId = p.MealPeriodId,
                            Date = startOfWeekUtc.AddDays(i)
                        };
                    }
                    allSlots.Add(slot);
                }
            }

            model.MealsToPeriods = allSlots;

            return View(model);
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> EditSchedule(int week = 0)
        {
            ViewBag.week = week;
            var startOfWeekCst = base.GetStartOfCurrentWeek().AddDays(7 * week);
            var startOfWeekUtc = base.ConvertCstToUtc(startOfWeekCst);
            var startOfNextWeekUtc = base.ConvertCstToUtc(startOfWeekCst.AddDays(7));

            var model = new MealScheduleModel();
            model.StartOfWeek = startOfWeekCst;
            model.Meals = await _db.Meals.ToListAsync();
            model.MealPeriods = await _db.MealPeriods.ToListAsync();
            model.UsersVotes = Request.IsAuthenticated
                ? (await _db.Members.FindAsync(WebSecurity.CurrentUserId)).MealVotes
                : new List<MealVote>();
            model.LatePlates = await _db.MealLatePlates
                .Where(m => m.MealToPeriod.Date >= startOfWeekUtc.Date && m.MealToPeriod.Date < startOfNextWeekUtc.Date)
                .ToListAsync();

            var existingAssignments = await _db.MealToPeriods
                .Where(m => m.Date >= startOfWeekUtc.Date && m.Date < startOfNextWeekUtc.Date)
                .ToListAsync();

            var allSlots = new List<MealToPeriod>();
            foreach (var p in model.MealPeriods)
            {
                for (var i = 0; i < 7; i++)
                {
                    var slot = existingAssignments
                        .SingleOrDefault(m =>
                            m.MealPeriodId == p.MealPeriodId &&
                            m.Date == startOfWeekUtc.AddDays(i).Date);
                    if (slot == null)
                    {
                        slot = new MealToPeriod
                        {
                            MealPeriodId = p.MealPeriodId,
                            Date = startOfWeekUtc.AddDays(i)
                        };
                    }
                    allSlots.Add(slot);
                }
            }

            model.MealsToPeriods = allSlots;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> EditSchedule(MealScheduleModel model, int week = 0)
        {
            if (!ModelState.IsValid) return RedirectToAction("Schedule", new { week });

            foreach (var i in model.MealsToPeriods)
            {
                var copy = i;
                var existingSlot = await _db.MealToPeriods
                    .SingleOrDefaultAsync(m => 
                        m.MealPeriodId == copy.MealPeriodId && m.Date == copy.Date);
                if (existingSlot == null)
                {
                    if (copy.MealId > 0)
                    {
                        _db.MealToPeriods.Add(i);
                    }
                }
                else
                {
                    if (i.MealId <= 0)
                    {
                        _db.Entry(existingSlot).State = EntityState.Deleted;
                    }
                    else
                    {
                        existingSlot.MealId = copy.MealId;
                        _db.Entry(existingSlot).State = EntityState.Modified;
                    }
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("EditSchedule", new { week });
        }

        public async Task<ActionResult> Upvote(int? id, int week = 0)
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

            var existingVote = await _db.MealVotes
                    .SingleOrDefaultAsync(v => 
                        v.UserId == WebSecurity.CurrentUserId && 
                        v.MealItemId == mealItem.MealItemId);

            if (existingVote == null)
            {
                _db.MealVotes.Add(new MealVote
                {
                    UserId = WebSecurity.CurrentUserId,
                    MealItemId = mealItem.MealItemId,
                    IsUpvote = true
                });
            }
            else
            {
                if (existingVote.IsUpvote)
                {
                    _db.Entry(existingVote).State = EntityState.Deleted;
                }
                else
                {
                    existingVote.IsUpvote = true;
                    _db.Entry(existingVote).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();
            if (week != 0)
            {
                return RedirectToAction("Schedule", new { week });
            }
            return Redirect(Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "Schedule");
        }

        public async Task<ActionResult> Downvote(int? id, int week = 0)
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

            var existingVote = await _db.MealVotes
                    .SingleOrDefaultAsync(v =>
                        v.UserId == WebSecurity.CurrentUserId &&
                        v.MealItemId == mealItem.MealItemId);

            if (existingVote == null)
            {
                _db.MealVotes.Add(new MealVote
                {
                    UserId = WebSecurity.CurrentUserId,
                    MealItemId = mealItem.MealItemId,
                    IsUpvote = false
                });
            }
            else
            {
                if (!existingVote.IsUpvote)
                {
                    _db.Entry(existingVote).State = EntityState.Deleted;
                }
                else
                {
                    existingVote.IsUpvote = false;
                    _db.Entry(existingVote).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();
            if (week != 0)
            {
                return RedirectToAction("Schedule", new { week });
            }
            return Redirect(Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "Schedule");
        }

        public async Task<ActionResult> LatePlateSignup(int? id, int week = 0)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mp = await _db.MealToPeriods.FindAsync(id);
            if (mp == null)
            {
                return HttpNotFound();
            }

            var existingLatePlate = await _db.MealLatePlates
                    .SingleOrDefaultAsync(v =>
                        v.UserId == WebSecurity.CurrentUserId &&
                        v.MealToPeriodId == mp.MealToPeriodId);

            if (existingLatePlate == null)
            {
                _db.MealLatePlates.Add(new MealLatePlate
                {
                    UserId = WebSecurity.CurrentUserId,
                    MealToPeriodId = mp.MealToPeriodId
                });
            }
            else
            {
                _db.Entry(existingLatePlate).State = EntityState.Deleted;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { week });
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Create()
        {
            var model = new CreateMealModel
            {
                MealItems = await base.GetMealItemsSelectListAsync()
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, House Steward")]
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

        [Authorize(Roles = "Administrator, House Steward")]
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
        [Authorize(Roles = "Administrator, House Steward")]
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

        [Authorize(Roles = "Administrator, House Steward")]
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
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var meal = await _db.Meals.FindAsync(id);
            _db.Meals.Remove(meal);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
