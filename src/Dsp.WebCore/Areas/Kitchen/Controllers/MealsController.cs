namespace Dsp.WebCore.Areas.Kitchen.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Exceptions;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Areas.Kitchen.Models;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

[Authorize]
public class MealsController : BaseController
{
    private readonly IMealService _mealService;
    private readonly IPositionService _positionService;

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
        var currentUser = await UserManager.GetUserAsync(User);
        var userRoles = await UserManager.GetRolesAsync(currentUser);
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
            UserId = User.GetUserId(),
            MealItemId = id,
            IsUpvote = true
        };
        await _mealService.ProcessVote(vote);

        return new StatusCodeResult((int) HttpStatusCode.OK);
    }

    public async Task<ActionResult> Downvote(int id, int week = 0)
    {
        var vote = new MealItemVote
        {
            UserId = User.GetUserId(),
            MealItemId = id,
            IsUpvote = false
        };
        await _mealService.ProcessVote(vote);

        return new StatusCodeResult((int) HttpStatusCode.OK);
    }

    [HttpPost]
    public async Task<ActionResult> AddPlate(DateTime dateTime, string type, int week = 0)
    {
        var plate = new MealPlate
        {
            PlateDateTime = dateTime,
            SignedUpOn = DateTime.UtcNow,
            UserId = User.GetUserId(),
            Type = type
        };

        await _mealService.CreatePlate(plate);

        return RedirectToAction("Index", new { week });
    }

    [HttpPost]
    public async Task<ActionResult> RemovePlate(int id, int week = 0)
    {
        var plate = await _mealService.GetPlateByIdAsync(id);

        if (plate == null) return NotFound();

        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "House Steward");
        if (!hasElevatedPermissions && plate.UserId != userId)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }

        await _mealService.DeletePlate(id);

        return RedirectToAction("Index", new { week });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> AddMealItemsToPeriod(MealItemToPeriod[] model)
    {
        var response = new List<MealItemToPeriodModel>();
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
                }
                catch (MealItemAlreadyAssignedException)
                {

                }
            }
        }
        catch (Exception)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }

        return Json(response);
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteMealItemFromPeriod(int id)
    {
        if (id <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        try
        {
            await _mealService.DeleteItemFromPeriod(id);
        }
        catch (Exception)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }

        return new StatusCodeResult((int) HttpStatusCode.OK);
    }
}
