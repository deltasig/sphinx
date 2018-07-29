namespace Dsp.Web.Api
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Web.Api.Models;
    using Dsp.Web.Extensions;
    using Microsoft.AspNet.Identity;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    [Authorize]
    [RoutePrefix("api/meals")]
    public class MealsController : ApiController
    {
        private SphinxDbContext _db;
        private IMealService _mealService;

        public MealsController()
        {
            _db = new SphinxDbContext();
            _mealService = new MealService(_db);
        }

        [Authorize]
        [HttpGet, Route("votes"), ResponseType(typeof(MealItemVote[]))]
        public async Task<IHttpActionResult> GetMealItemVotesForCurrentUser()
        {
            var response = new List<MealItemVoteApiModel>();
            try
            {
                var votes = await _mealService.GetAllVotesByUserIdAsync(User.Identity.GetUserId<int>());
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
        [HttpGet, Route("plates"), ResponseType(typeof(MealItemVote[]))]
        public async Task<IHttpActionResult> GetMealPlates(int week = 0)
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
}
