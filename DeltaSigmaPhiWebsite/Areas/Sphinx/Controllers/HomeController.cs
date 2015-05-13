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
            ViewBag.FailMessage = message;
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            var member = await _db.Members.FindAsync(userId);
            var events = await base.GetAllCompletedEventsForUserAsync(userId);
            var thisSemester = await base.GetThisSemesterAsync();

            var thisWeeksSoberShifts = await base.GetThisWeeksSoberSignupsAsync(DateTime.UtcNow);
            var memberSoberSignups = await base.GetSoberSignupsForUserAsync(userId, thisSemester);
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
                Roles = Roles.GetRolesForUser(member.UserName),
                RemainingCommunityServiceHours = await GetRemainingServiceHoursForUserAsync(userId),
                StudyHourAssignments = await GetStudyHourAssignmentsForUserAsync(userId, thisSemester),
                CompletedEvents = events,
                SoberSignups = thisWeeksSoberShifts,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any() && remainingDriverShifts.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = await GetLastSemesterAsync(),
                ApprovalRequests = await _db.StudyHours.Where(s => s.ApproverId == userId && s.DateTimeApproved == null).ToListAsync()
            };

            return View(model);
        }
    }
}