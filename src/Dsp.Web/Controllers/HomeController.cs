namespace Dsp.Web.Controllers
{
    using Data.Entities;
    using Dsp.Services;
    using Extensions;
    using MarkdownSharp;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [AllowAnonymous, RequireHttps]
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> Contacts()
        {
            var term = await GetCurrentTerm();
            return View(term);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Recruitment()
        {
            var model = new RecruitmentModel();
            model.ScholarshipApps = await _db.ScholarshipApps
                .Where(m =>
                    m.IsPublic &&
                    m.Type.Name == "Building Better Men Scholarship")
                .ToListAsync();
            model.Semester = await _db.Semesters
                .Where(m => !string.IsNullOrEmpty(m.RecruitmentBookUrl))
                .OrderByDescending(m => m.DateEnd)
                .FirstOrDefaultAsync();

            return View(model);
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
        public ActionResult Alumni()
        {
            return RedirectToAction("Index", "Home", new { area = "Alumni" });
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            var model = new AboutModel();
            var markdown = new Markdown();
            var data = System.IO.File.ReadAllText(Server.MapPath(@"~/Documents/History.md"));
            model.History = markdown.Transform(data);
            data = System.IO.File.ReadAllText(Server.MapPath(@"~/Documents/Awards.md"));
            model.Awards = markdown.Transform(data);

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Donate()
        {
            var model = new DonationPledgeModel();
            var treasuryService = new TreasuryService();
            model.ActiveFundraisers = await treasuryService.GetActiveFundraisersAsync();
            var pledgeableFundraisers = model.ActiveFundraisers.Where(m => m.IsPledgeable);
            model.PledgeableFundraisers = GetFundraiserSelectList(pledgeableFundraisers);
            model.Amount = 5;

            return View(model);
        }

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Donate(DonationPledgeModel model)
        {
            var treasuryService = new TreasuryService();
            if (!ModelState.IsValid)
            {
                model.ActiveFundraisers = (await treasuryService.GetActiveFundraisersAsync());
                var pledgeableFundraisers = model.ActiveFundraisers.Where(m => m.IsPledgeable);
                model.PledgeableFundraisers = GetFundraiserSelectList(pledgeableFundraisers);
                return View(model);
            }

            if (string.IsNullOrEmpty(model.PhoneNumber) && string.IsNullOrEmpty(model.Email))
            {
                var failureMessage = "Your donation pledge must contain either an email or " +
                       "phone number so we can contact you later.";
                ModelState.AddModelError(string.Empty, failureMessage);

                model.ActiveFundraisers = (await treasuryService.GetActiveFundraisersAsync());
                var pledgeableFundraisers = model.ActiveFundraisers.Where(m => m.IsPledgeable);
                model.PledgeableFundraisers = GetFundraiserSelectList(pledgeableFundraisers);
                return View(model);
            }

            var donation = new Donation
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Amount = model.Amount,
                FundraiserId = model.FundraiserId
            };
            await treasuryService.CreateDonationAsync(donation);

            donation.Fundraiser = await treasuryService.GetFundraiserByIdAsync(donation.FundraiserId);

            return View("DonationConfirmation", donation);
        }

        [AllowAnonymous]
        public async Task<ActionResult> EmailSoberSchedule()
        {
            var isPermitted = (User.IsInRole("Administrator") || User.IsInRole("Sergeant-at-Arms"));
            var result = await EmailService.TryToSendSoberSchedule(new SoberService(_db), _db, isPermitted);
            return Content(result);
        }

        [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate"), HttpGet]
        public async Task<ActionResult> Sphinx()
        {
            var nowCst = DateTime.UtcNow.FromUtcToCst();
            var twoHoursAgoCst = nowCst.AddHours(-2);
            var member = await UserManager.FindByNameAsync(User.Identity.Name);
            var events = await GetAllCompletedEventsForUserAsync(member.Id);
            var thisSemester = await GetThisSemesterAsync();
            var lastSemester = await GetLastSemesterAsync();
            var soberService = new SoberService(_db);

            var thisWeeksSoberShifts = await soberService.GetUpcomingSignupsAsync();
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

        [HttpGet]
        public ActionResult Updates()
        {
            var markdown = new Markdown();
            var data = System.IO.File.ReadAllText(Server.MapPath(@"~/Documents/Updates.md"));
            var content = markdown.Transform(data);
            return View("Updates", (object)content);
        }

        private SelectList GetFundraiserSelectList(IEnumerable<Fundraiser> fundraisers)
        {
            var items = new List<SelectListItem>();
            foreach (var f in fundraisers)
            {
                items.Add(new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name + " Fundraiser for " + f.Cause.Name
                });
            }
            return new SelectList(items, "Value", "Text");
        }
    }
}
