namespace Dsp.Web.Controllers
{
    using Data.Entities;
    using Extensions;
    using MarkdownSharp;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.UI;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate"), RequireHttps]
    public class HomeController : BaseController
    {
        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public async Task<ActionResult> Contacts()
        {
            var term = await GetCurrentTerm();
            return View(term);
        }

        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public async Task<ActionResult> Recruitment()
        {
            return View(await _db.ScholarshipApps
                .Where(s => s.IsPublic && s.Type.Name == "Building Better Men Scholarship").ToListAsync());
        }

        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public async Task<ActionResult> Scholarships()
        {
            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            var model = new ExternalScholarshipModel
            {
                Applications = await _db.ScholarshipApps.ToListAsync(),
                Types = await _db.ScholarshipTypes.ToListAsync()
            };

            var markdown = new Markdown();
            foreach (var app in model.Applications)
            {
                app.AdditionalText = markdown.Transform(app.AdditionalText);
            }

            return View(model);
        }

        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public ActionResult Alumni()
        {
            return RedirectToAction("Index", "Home", new { area = "Alumni" });
        }

        [AllowAnonymous, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public ActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> EmailSoberSchedule()
        {
            var nowUtc = DateTime.UtcNow;
            var nowCst = ConvertUtcToCst(nowUtc);

            var type = await _db.EmailTypes.SingleOrDefaultAsync(e => e.Name == "Sober Schedule");
            if (string.IsNullOrEmpty(type?.Destination))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var emails = await _db.Emails
                .Where(e =>
                    e.EmailTypeId == type.EmailTypeId &&
                    e.Destination == type.Destination)
                .OrderByDescending(e => e.SentOn)
                .ToListAsync();
            var mostRecentEmail = emails.FirstOrDefault();

            // Check if it has been over 24 hours since the last email.
            var noPreviousEmail = mostRecentEmail == null || (nowUtc - mostRecentEmail.SentOn).TotalHours > 24;
            // Check if the current time is between the arbitrary range.
            var isTime = (nowCst.DayOfWeek == DayOfWeek.Friday &&
                          nowCst.Hour >= 16 && nowCst.Hour < 19);
            // If an admin or the sergeant is trying to manually send the email, just allow it.
            var canOverride = (User.IsInRole("Administrator") || User.IsInRole("Sergeant-at-Arms"));

            // Don't send the email if conditions aren't right.
            if ((!isTime || !noPreviousEmail) && !canOverride)
            {
                return Content("Time: " + isTime + ", Email: " + noPreviousEmail);
            }

            // Build Body
            var data = await GetUpcomingSoberSignupsAsync(nowUtc);

            if (!data.Any())
            {
                return Content("No sober signups found; no email sent.");
            }

            var body = RenderRazorViewToString("~/Views/Emails/SoberSchedule.cshtml", data);
            var bytes = Encoding.Default.GetBytes(body);
            body = Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = "Sober Schedule: " +
                nowCst.ToShortDateString() + " - " + nowCst.AddDays(7).ToShortDateString(),
                Body = body,
                Destination = type.Destination
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendTemplatedAsync(message);
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            var email = new Email
            {
                SentOn = nowUtc,
                EmailTypeId = type.EmailTypeId,
                Destination = type.Destination,
                Body = body
            };

            _db.Emails.Add(email);
            await _db.SaveChangesAsync();

            return Content("OK");
        }

        [HttpGet]
        [OutputCache(Duration = 60, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public async Task<ActionResult> Sphinx()
        {
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

            var model = new SphinxModel
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

            var mostRecentIncident = await _db.IncidentReports
                .OrderByDescending(i => i.DateTimeOfIncident)
                .FirstOrDefaultAsync() ?? new IncidentReport();
            var startOfYearUtc = ConvertCstToUtc(new DateTime(nowCst.Year, 1, 1));
            model.DaysSinceIncident = (DateTime.UtcNow - mostRecentIncident.DateTimeSubmitted).Days;
            model.IncidentsThisSemester = await _db.IncidentReports.CountAsync(i => i.DateTimeOfIncident > lastSemester.DateEnd);
            model.ScholarshipSubmissionsThisYear = await _db.ScholarshipSubmissions.CountAsync(s => s.SubmittedOn >= startOfYearUtc);
            model.LaundryUsageThisSemester = await _db.LaundrySignups.CountAsync(l => l.DateTimeShift >= thisSemester.DateStart);
            model.NewMembersThisSemester = await _db.Users.CountAsync(u => u.PledgeClass.SemesterId == thisSemester.SemesterId);
            model.ServiceHoursThisSemester = 0;
            var members = await GetRosterForSemester(thisSemester);
            foreach (var m in members)
            {
                var serviceHours = m.ServiceHours
                    .Where(e =>
                        e.Event.DateTimeOccurred > lastSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= thisSemester.DateEnd &&
                        e.Event.IsApproved).Sum(e => e.DurationHours);
                model.ServiceHoursThisSemester += serviceHours;
            }

            return View(model);
        }

        [HttpGet, Authorize, OutputCache(Duration = 3600, Location = OutputCacheLocation.Any, VaryByParam = "none")]
        public ActionResult Updates()
        {
            var markdown = new Markdown();
            var data = System.IO.File.ReadAllText(Server.MapPath(@"~/Documents/Updates.md"));
            var content = markdown.Transform(data);
            return View("Updates", (object)content);
        }
    }
}
