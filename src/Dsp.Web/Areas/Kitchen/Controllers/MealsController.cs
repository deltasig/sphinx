namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

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
            var startOfWeekCst = GetStartOfCurrentWeek().AddDays(7 * week);
            var startOfWeekUtc = ConvertCstToUtc(startOfWeekCst);
            ViewBag.StartOfWeek = startOfWeekUtc;
            var startOfNextWeekUtc = ConvertCstToUtc(startOfWeekCst.AddDays(7));

            var model = new MealScheduleModel();
            model.StartOfWeek = startOfWeekCst;
            model.Meals = await _db.Meals.ToListAsync();
            model.MealPeriods = await _db.MealPeriods.ToListAsync();
            model.UsersVotes = Request.IsAuthenticated 
                ? (await UserManager.FindByIdAsync(User.Identity.GetUserId<int>())).MealItemVotes 
                : new List<MealItemVote>();
            model.Plates = await _db.MealPlates
                .Where(m => 
                    m.PlateDateTime >= startOfWeekUtc.Date && 
                    m.PlateDateTime < startOfNextWeekUtc.Date)
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
                            m.MealPeriodId == p.Id && 
                            m.Date == startOfWeekUtc.AddDays(i).Date);
                    if (slot == null)
                    {
                        slot = new MealToPeriod
                        {
                            MealPeriodId = p.Id,
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
            var startOfWeekCst = GetStartOfCurrentWeek().AddDays(7 * week);
            var startOfWeekUtc = ConvertCstToUtc(startOfWeekCst);
            var startOfNextWeekUtc = ConvertCstToUtc(startOfWeekCst.AddDays(7));

            var model = new MealScheduleModel();
            model.StartOfWeek = startOfWeekCst;
            model.Meals = await _db.Meals.ToListAsync();
            model.MealPeriods = await _db.MealPeriods.ToListAsync();
            model.UsersVotes = Request.IsAuthenticated
                ? (await UserManager.FindByIdAsync(User.Identity.GetUserId<int>())).MealItemVotes
                : new List<MealItemVote>();

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
                            m.MealPeriodId == p.Id &&
                            m.Date == startOfWeekUtc.AddDays(i).Date);
                    if (slot == null)
                    {
                        slot = new MealToPeriod
                        {
                            MealPeriodId = p.Id,
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

            var userId = User.Identity.GetUserId<int>();
            var existingVote = await _db.MealVotes
                    .SingleOrDefaultAsync(v => 
                        v.UserId == userId && 
                        v.MealItemId == mealItem.Id);

            if (existingVote == null)
            {
                _db.MealVotes.Add(new MealItemVote
                {
                    UserId = userId,
                    MealItemId = mealItem.Id,
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

            var userId = User.Identity.GetUserId<int>();
            var existingVote = await _db.MealVotes
                    .SingleOrDefaultAsync(v =>
                        v.UserId == userId &&
                        v.MealItemId == mealItem.Id);

            if (existingVote == null)
            {
                _db.MealVotes.Add(new MealItemVote
                {
                    UserId = userId,
                    MealItemId = mealItem.Id,
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPlate(DateTime dateTime, string type, int week = 0)
        {
            var plate = new MealPlate
            {
                PlateDateTime = dateTime,
                SignedUpOn = DateTime.UtcNow,
                UserId = User.Identity.GetUserId<int>()
            };

            var existingPlates = await _db.MealPlates
                    .Where(v => v.UserId == plate.UserId &&
                        v.PlateDateTime == dateTime)
                    .ToListAsync();

            if (type == "Late" && existingPlates.Any(p => p.Type == "Late"))
            {
                plate.Type = "+1";
            }
            else
            {
                plate.Type = type;
            }

            _db.MealPlates.Add(plate);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { week });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePlate(int id, int week = 0)
        {
            var plate = await _db.MealPlates.FindAsync(id);
            if (plate == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Administrator") && !User.IsInRole("House Steward") &&
                plate.UserId != User.Identity.GetUserId<int>())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _db.MealPlates.Remove(plate);
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
                    MealId = meal.Id, 
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
                SelectedMealItemIds = meal.MealItems.Select(m => m.MealItemId).ToArray(),
                MealId = meal.Id
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

            foreach (var i in meal.MealItems.ToList())
            {
                if (!model.SelectedMealItemIds.Contains(i.MealItemId))
                {
                    _db.Entry(i).State = EntityState.Deleted;
                }
            }
            var remainingIds = meal.MealItems.Select(m => m.MealItemId).ToList();
            foreach (var i in model.SelectedMealItemIds)
            {
                if (remainingIds.Contains(i)) continue;
                var mealItem = new MealToItem
                {
                    MealId = meal.Id, 
                    MealItemId = i
                };
                meal.MealItems.Add(mealItem);
            }

            _db.Entry(meal).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Reorder(int? id)
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

            return View(meal.MealItems.OrderBy(i => i.DisplayOrder).ToList());
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Reorder(IList<MealToItem> model)
        {
            if (!ModelState.IsValid || !model.Any()) return RedirectToAction("Index", new {  });

            var items = model.OrderBy(m => m.DisplayOrder).ToList();
            for (var i = 0; i < model.Count; i++)
            {
                items[i].DisplayOrder = i;
                _db.Entry(items[i]).State = EntityState.Modified;
            }
            
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
