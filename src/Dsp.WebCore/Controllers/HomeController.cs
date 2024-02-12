namespace Dsp.WebCore.Controllers;

using Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Extensions;
using MarkdownSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route(""), AllowAnonymous, RequireHttps]
public class HomeController : BaseController
{
    readonly IWebHostEnvironment _env;
    private readonly ISemesterService _semesterService;
    private readonly ISoberService _soberService;
    private readonly IMemberService _memberService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public HomeController(IWebHostEnvironment env,
        ISemesterService semesterService, ISoberService soberService, IMemberService memberService,
        IUserService userService, IRoleService roleService)
    {
        _env = env;
        _semesterService = semesterService;
        _soberService = soberService;
        _memberService = memberService;
        _userService = userService;
        _roleService = roleService;
    }

    [Route("")]
    public ActionResult Index()
    {
        return View();
    }

    [Route("Donate")]
    public ActionResult Donate()
    {
        return View();
    }

    [Route("Joining")]
    public async Task<ActionResult> Joining()
    {
        var scholarshipApps = await Context.ScholarshipApps
            .Where(m =>
                m.IsPublic &&
                m.Type.Name == "Building Better Men Scholarship")
            .ToListAsync();
        var mostRecentSemesterWithRecruitmentBook = await Context.Semesters
            .Where(m => !string.IsNullOrEmpty(m.RecruitmentBookUrl))
            .OrderByDescending(m => m.DateEnd)
            .FirstOrDefaultAsync();

        var model = new RecruitmentModel();
        model.ScholarshipApps = scholarshipApps;
        model.Semester = mostRecentSemesterWithRecruitmentBook;

        return View(model);
    }

    [Route("Scholarships")]
    public async Task<ActionResult> Scholarships()
    {
        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];

        var model = new ExternalScholarshipModel
        {
            Applications = await Context.ScholarshipApps.ToListAsync(),
            Types = await Context.ScholarshipTypes.ToListAsync()
        };

        var markdown = new Markdown();
        foreach (var app in model.Applications)
        {
            app.AdditionalText = markdown.Transform(app.AdditionalText);
        }

        return View(model);
    }

    [Route("History")]
    public ActionResult History()
    {
        var model = new AboutModel();
        var markdown = new Markdown();
        var historyPath = Path.Combine(_env.ContentRootPath, "Documents/History.md");
        var data = System.IO.File.ReadAllText(historyPath);
        model.History = markdown.Transform(data);

        var awardsPath = Path.Combine(_env.ContentRootPath, "Documents/Awards.md");
        data = System.IO.File.ReadAllText(awardsPath);
        model.Awards = markdown.Transform(data);

        return View(model);
    }

    [Route("Contacts")]
    public async Task<ActionResult> Contacts()
    {
        var term = await _semesterService.GetCurrentSemesterAsync();
        return View(term);
    }

    [Route("EmailSoberSchedule")]
    public async Task<ActionResult> EmailSoberSchedule()
    {
        // TODO: Email sober schedule
        var result = "";
        return Content(result);
    }

    [Route("Sphinx"), Authorize]
    public async Task<ActionResult> Sphinx()
    {
        var nowCst = DateTime.UtcNow.FromUtcToCst();
        var twoHoursAgoCst = nowCst.AddHours(-2);
        var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);
        var roles = (await _roleService.GetCurrentRolesByUserIdAsync(user.Id))
            .Select(x => x.Name)
            .ToList() ?? new List<string>();
        var events = await _memberService.GetAllCompletedEventsForUserAsync(user.Id);
        var remainingServiceHours = await _memberService.GetRemainingServiceHoursForUserAsync(user.Id);
        var thisSemester = await _semesterService.GetCurrentSemesterAsync();
        var prevSemester = await _semesterService.GetPriorSemesterAsync(thisSemester);

        var thisWeeksSoberShifts = await _soberService.GetUpcomingSignupsAsync();
        var memberSoberSignups = await _memberService.GetSoberSignupsForUserAsync(user.Id, thisSemester);
        var remainingDriverShifts = await Context.SoberSignups
            .Where(s =>
                s.UserId == null &&
                s.SoberType.Name == "Driver" &&
                s.DateOfShift >= DateTime.UtcNow &&
                s.DateOfShift <= thisSemester.DateEnd)
            .ToListAsync();

        var laundrySignups = await Context.LaundrySignups
            .Where(l => l.DateTimeShift >= twoHoursAgoCst)
            .OrderBy(l => l.DateTimeShift)
            .ToListAsync();
        var laundryTake = laundrySignups.Count > 5 ? 5 : laundrySignups.Count;

        var model = new SphinxModel
        {
            MemberInfo = user.MemberInfo,
            Roles = roles,
            RemainingCommunityServiceHours = remainingServiceHours,
            CompletedEvents = events,
            SoberSignups = thisWeeksSoberShifts,
            LaundrySummary = laundrySignups.Take(laundryTake),
            NeedsToSoberDrive = !memberSoberSignups.Any() && remainingDriverShifts.Any(),
            CurrentSemester = thisSemester,
            PreviousSemester = prevSemester
        };

        var mostRecentIncident = await Context.IncidentReports
            .OrderByDescending(i => i.DateTimeOfIncident)
            .FirstOrDefaultAsync() ?? new IncidentReport();
        var startOfYearUtc = new DateTime(nowCst.Year, 1, 1).FromCstToUtc();
        model.DaysSinceIncident = (DateTime.UtcNow - mostRecentIncident.DateTimeSubmitted).Days;
        model.IncidentsThisSemester = await Context.IncidentReports.CountAsync(i => i.DateTimeOfIncident > prevSemester.DateEnd);
        model.ScholarshipSubmissionsThisYear = await Context.ScholarshipSubmissions.CountAsync(s => s.SubmittedOn >= startOfYearUtc);
        model.LaundryUsageThisSemester = await Context.LaundrySignups.CountAsync(l => l.DateTimeShift >= thisSemester.DateStart);
        model.NewMembersThisSemester = await Context.Members.CountAsync(u => u.PledgeClass.SemesterId == thisSemester.Id);
        model.ServiceHoursThisSemester = 0;
        //var members = await GetRosterForSemester(thisSemester);
        //foreach (var m in members)
        //{
        //    var serviceHours = m.ServiceHours
        //        .Where(e =>
        //            e.Event.DateTimeOccurred > lastSemester.DateEnd &&
        //            e.Event.DateTimeOccurred <= thisSemester.DateEnd &&
        //            e.Event.IsApproved).Sum(e => e.DurationHours);
        //    model.ServiceHoursThisSemester += serviceHours;
        //}

        return View(model);
    }
}
