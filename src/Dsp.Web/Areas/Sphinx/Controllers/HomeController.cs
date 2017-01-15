namespace Dsp.Web.Areas.Sphinx.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using Models;
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

            var nowCst = ConvertUtcToCst(DateTime.UtcNow);
            var twoHoursAgoCst = nowCst.AddHours(-2);
            var member = await UserManager.FindByNameAsync(User.Identity.Name);
            var events = await GetAllCompletedEventsForUserAsync(member.Id);
            var thisSemester = await GetThisSemesterAsync();
            var lastSemester = await GetLastSemesterAsync();

            var thisWeeksSoberShifts = await GetUpcomingSoberSignupsAsync();
            var memberSoberSignups = await GetSoberSignupsForUserAsync(member.Id, thisSemester);
            var remainingDriverShifts = await _db.SoberSignups
                .Where(s => 
                    s.UserId == null &&
                    s.SoberType.Name == "Driver" &&
                    s.DateOfShift >= DateTime.UtcNow &&
                    s.DateOfShift <= thisSemester.DateEnd)
                .ToListAsync();

            var laundrySignups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= twoHoursAgoCst)
                .OrderBy(l => l.DateTimeShift)
                .ToListAsync();
            var laundryTake = laundrySignups.Count > 5 ? 5 : laundrySignups.Count;

            var model = new SphinxHomeIndexModel
            {
                MemberInfo = member,
                Roles = await UserManager.GetRolesAsync(member.Id),
                RemainingCommunityServiceHours = await GetRemainingServiceHoursForUserAsync(member.Id),
                CompletedEvents = events,
                SoberSignups = thisWeeksSoberShifts,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any() && remainingDriverShifts.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = await GetLastSemesterAsync()
            };
            if (member.MemberStatus.StatusName == "Alumnus")
            {
                var mostRecentIncident = await _db.IncidentReports
                    .OrderByDescending(i => i.DateTimeOfIncident)
                    .FirstOrDefaultAsync() ?? new IncidentReport();
                var startOfYearUtc = ConvertCstToUtc(new DateTime(nowCst.Year, 1, 1));
                var serviceHoursSoFar = await _db.ServiceHours.Where(s => s.DateTimeSubmitted >= thisSemester.DateStart).ToListAsync();
                model.DaysSinceIncident = (DateTime.UtcNow - mostRecentIncident.DateTimeSubmitted).Days;
                model.IncidentsThisSemester = await _db.IncidentReports.CountAsync(i => i.DateTimeOfIncident > lastSemester.DateEnd);
                model.ScholarshipSubmissionsThisYear = await _db.ScholarshipSubmissions.CountAsync(s => s.SubmittedOn >= startOfYearUtc);
                model.LaundryUsageThisSemester = await _db.LaundrySignups.CountAsync(l => l.DateTimeShift >= thisSemester.DateStart);
                model.ServiceHoursThisSemester = serviceHoursSoFar.Sum(s => s.DurationHours);
                model.NewMembersThisSemester = await _db.Users.CountAsync(u => u.PledgeClass.SemesterId == thisSemester.SemesterId);
            }

            return View(model);
        }
    }
}