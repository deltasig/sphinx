namespace Dsp.WebCore.Api;

using Data;
using Dsp.Data.Entities;
using Dsp.WebCore.Api.Models;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/meals")]
public class MealsController : ControllerBase
{
    private DspDbContext _db;
    private IMealService _mealService;
    private readonly UserManager<User> _userManager;

    public MealsController(DspDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _mealService = new MealService(db);
        _userManager = userManager;
    }

    [Authorize]
    [Route("~/api/meals/votes")]
    public async Task<IActionResult> GetMealItemVotesForCurrentUser()
    {
        var response = new List<MealItemVoteApiModel>();
        try
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var votes = await _mealService.GetAllVotesByUserIdAsync(currentUser.Id);
            foreach (var vote in votes)
            {
                response.Add(new MealItemVoteApiModel
                {
                    MealItemId = vote.MealItemId,
                    IsUpvote = vote.IsUpvote
                });
            }
        }
        catch (Exception)
        {
            return BadRequest("Failed to get meal item votes for current user!");
        }

        return Ok(response);
    }

    [Authorize]
    [Route("~/api/meals/plates")]
    public async Task<IActionResult> GetMealPlates(int week = 0)
    {
        var nowUtc = DateTime.UtcNow.AddDays(week * 7);
        var weekOfYear = DateTimeExtensions.GetWeekOfYear(nowUtc);
        var startDate = DateTimeExtensions.FirstDateOfWeek(nowUtc.Year, weekOfYear);
        var endDate = startDate.AddDays(7);
        IEnumerable<MealPlate> response;
        try
        {
            response = await _mealService.GetMealPlatesAsync(startDate, endDate);
        }
        catch (Exception)
        {
            return BadRequest("Failed to get meal plates!");
        }

        return Ok(response);
    }
}
