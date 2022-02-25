namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Exceptions;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Areas.Kitchen.Models;
    using Dsp.Web.Controllers;
    using Dsp.Web.Extensions;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize(Roles = "Alumnus, Active, Neophyte, New, Affiliate")]
    public class MealsController : BaseController
    {
        private readonly IMealService _mealService;
        private readonly IPositionService _positionService;

        public MealsController()
        {
            var repo = new Repository<SphinxDbContext>(_db);
            _mealService = new MealService(repo);
            _positionService = new PositionService(repo);
        }

        public MealsController(IMealService mealService, IPositionService positionService)
        {
            _mealService = mealService;
            _positionService = positionService;
        }

        public async Task<ActionResult> Index(int week = 0)
        {
            var nowUtc = DateTime.UtcNow.AddDays(week * 7);
            var weekOfYear = DateTimeExtensions.GetWeekOfYear(nowUtc);
            var startDate = DateTimeExtensions.FirstDateOfWeek(nowUtc.Year, weekOfYear);
            var endDate = startDate.AddDays(7);

            var periods = await _mealService.GetAllPeriodsAsync();
            var mpItems = await _mealService.GetMealItemToPeriodsAsync(startDate, endDate);
            var plates = await _mealService.GetMealPlatesAsync(startDate, endDate);
            var userRoles = Roles.GetRolesForUser();
            var hasElevatedPermissions = userRoles.Any(r => r == "Administrator" || r == "House Steward");

            var model = new MealIndexModel(periods, mpItems, plates, startDate, hasElevatedPermissions)
            {
                WeekOffset = week
            };
            if (hasElevatedPermissions)
            {
                model.MealItems = await _mealService.GetAllItemsAsync();
            }

            return View(model);
        }

        public async Task<ActionResult> Upvote(int id, int week = 0)
        {
            var vote = new MealItemVote
            {
                UserId = User.Identity.GetUserId<int>(),
                MealItemId = id,
                IsUpvote = true
            };
            await _mealService.ProcessVote(vote);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public async Task<ActionResult> Downvote(int id, int week = 0)
        {
            var vote = new MealItemVote
            {
                UserId = User.Identity.GetUserId<int>(),
                MealItemId = id,
                IsUpvote = false
            };
            await _mealService.ProcessVote(vote);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<ActionResult> AddPlate(DateTime dateTime, string type, int week = 0)
        {
            var plate = new MealPlate
            {
                PlateDateTime = dateTime,
                SignedUpOn = DateTime.UtcNow,
                UserId = User.Identity.GetUserId<int>(),
                Type = type
            };

            await _mealService.CreatePlate(plate);

            return RedirectToAction("Index", new { week });
        }

        [HttpPost]
        public async Task<ActionResult> RemovePlate(int id, int week = 0)
        {
            var plate = await _mealService.GetPlateByIdAsync(id);

            if (plate == null) return HttpNotFound();

            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "House Steward");
            if (!hasElevatedPermissions && plate.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            await _mealService.DeletePlate(id);

            return RedirectToAction("Index", new { week });
        }

        [HttpPost, Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> AddMealItemsToPeriod(MealItemToPeriod[] model)
        {
            var response = new List<MealItemToPeriodModel>();
            var itemsWereRemoved = false;
            try
            {
                foreach (var mealItemToPeriod in model)
                {
                    try
                    {
                        await _mealService.CreateItemToPeriod(mealItemToPeriod);
                        var mealItem = await _mealService.GetItemByIdAsync(mealItemToPeriod.MealItemId);

                        var responseData = new MealItemToPeriodModel
                        {
                            Id = mealItemToPeriod.Id,
                            MealItemId = mealItemToPeriod.MealItemId,
                            MealPeriodId = mealItemToPeriod.MealPeriodId,
                            MealItemName = mealItem.Name,
                        };
                        response.Add(responseData);
                        itemsWereRemoved = true;
                    }
                    catch (MealItemAlreadyAssignedException)
                    {

                    }
                }

                if (itemsWereRemoved) Response.RemoveOutputCacheItem(Url.Action("Index"));
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Failed to add meal items to period!");
            }

            return Json(response);
        }

        [HttpDelete, Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> DeleteMealItemFromPeriod(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Improper id received!");

            try
            {
                await _mealService.DeleteItemFromPeriod(id);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Failed to delete meal item from period!");
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
