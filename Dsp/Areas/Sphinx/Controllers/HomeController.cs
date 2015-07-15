namespace Dsp.Areas.Sphinx.Controllers
{
    using Dsp.Models;
    using global::Dsp.Controllers;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class HomeController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string message = "")
        {
            ViewBag.Message = message;

            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            var member = await UserManager.FindByIdAsync(user.Id);
            var events = await GetAllCompletedEventsForUserAsync(user.Id);
            var thisSemester = await GetThisSemesterAsync();

            var thisWeeksSoberShifts = await GetThisWeeksSoberSignupsAsync(DateTime.UtcNow);
            var memberSoberSignups = await GetSoberSignupsForUserAsync(user.Id, thisSemester);
            var remainingDriverShifts = await _db.SoberSignups
                .Where(s => 
                    s.UserId == null &&
                    s.SoberType.Name == "Driver" &&
                    s.DateOfShift >= DateTime.UtcNow &&
                    s.DateOfShift <= thisSemester.DateEnd)
                .ToListAsync();

            var laundrySignups = await _db.LaundrySignups
                    .Where(l => l.DateTimeShift >= DateTime.UtcNow)
                    .OrderBy(l => l.DateTimeShift)
                    .ToListAsync();
            var laundryTake = laundrySignups.Count;
            if (laundrySignups.Count > 5)
            {
                laundryTake = 5;
            }

            var model = new SphinxHomeIndexModel
            {
                MemberInfo = member,
                Roles = await UserManager.GetRolesAsync(member.Id),
                RemainingCommunityServiceHours = await GetRemainingServiceHoursForUserAsync(user.Id),
                CompletedEvents = events,
                SoberSignups = thisWeeksSoberShifts,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any() && remainingDriverShifts.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = await GetLastSemesterAsync(),
            };

            return View(model);
        }
    }
}