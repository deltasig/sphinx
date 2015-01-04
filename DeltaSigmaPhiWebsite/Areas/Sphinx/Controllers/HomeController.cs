namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class HomeController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string message = "")
        {
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            var member = await _db.Members.FindAsync(userId);
            var events = await GetAllCompletedEventsForUserAsync(userId);
            var startOfTodayCst = ConvertUtcToCst(DateTime.UtcNow).Date;
            var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            var sevenDaysAheadOfToday = startOfTodayUtc.AddDays(7);

            if (startOfTodayCst.Hour < 6)
            {
                startOfTodayUtc = startOfTodayUtc.AddDays(-1);
            }

            var soberSignups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= startOfTodayUtc && 
                            s.DateOfShift <= sevenDaysAheadOfToday)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.Type)
                .ToListAsync();

            var thisSemester = await GetThisSemesterAsync();
            var memberSoberSignups = await GetSoberSignupsForUserAsync(userId, thisSemester);

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
                Roles = Roles.GetRolesForUser(member.UserName),
                RemainingCommunityServiceHours = await GetRemainingServiceHoursForUserAsync(userId),
                MemberStudyHourAssignments = await GetStudyHourAssignmentsForUserAsync(userId, thisSemester),
                CompletedEvents = events,
                SoberSignups = soberSignups,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = await GetLastSemesterAsync(),
                ApprovalRequests = await _db.StudyHours.Where(s => s.ApproverId == userId && s.DateTimeApproved == null).ToListAsync()
            };

            ViewBag.Message = message;

            return View(model);
        }
    }
}