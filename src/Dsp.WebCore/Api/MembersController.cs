namespace Dsp.WebCore.Api
{
    using Data;
    using Data.Entities;
    using Dsp.WebCore.Models;
    using Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Authorize]
    [ApiController]
    [Route("api/members")]
    public class MembersController : ControllerBase
    {
        private DspDbContext _context;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private readonly UserManager<Member> _userManager;

        public MembersController(DspDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _semesterService = new SemesterService(context);
            _memberService = new MemberService(context);
            _userManager = userManager;
        }

        [Authorize(Roles = "New, Active, Alumnus")]
        [Route("~/api/members/roster/{sid:int}")]
        public async Task<IActionResult> Roster(int sid)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            if (semester == null) return NotFound();

            try
            {
                var roster = await _memberService.GetRosterForSemesterAsync(sid);
                return Ok(roster.Select(m => new
                {
                    id = m.Id,
                    firstName = m.FirstName,
                    lastName = m.LastName
                }));
            }
            catch (Exception)
            {
                return BadRequest("Roster acquisition failed.");
            }
        }

        [Authorize(Roles = "New, Active, Alumnus")]
        [Route("~/api/members/alumni/{sid:int}")]
        public async Task<IActionResult> Alumni(int sid)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            if (semester == null) return NotFound();

            try
            {
                var roster = await _memberService.GetAlumniAsync(sid);
                return Ok(roster.Select(m => new
                {
                    id = m.Id,
                    firstName = m.FirstName,
                    lastName = m.LastName
                }));
            }
            catch (Exception)
            {
                return BadRequest("Roster acquisition failed.");
            }
        }

        [Authorize(Roles = "New, Active, Alumnus")]
        [Route("~/api/members/feed/{page:int}")]
        public async Task<IActionResult> Feed(int page = 0)
        {
            const int pageLength = 10;
            const int feedLengthDays = 30;
            // Limit page minimum and maximum to [0, 10] (Can't go more than 10 weeks back if pageLength equals 7)
            page = page > 10 ? 10 : page;
            page = page < 0 ? 0 : page;
            var nowUtc = DateTime.UtcNow;
            var nowCst = nowUtc.FromUtcToCst();
            var startOfFeedUtc = nowUtc.AddDays(-feedLengthDays);
            var startOfFeedCst = nowCst.AddDays(-feedLengthDays);
            var thisSemester = await _semesterService.GetCurrentSemesterAsync();
            var model = new List<FeedItem>();

            // Laundry reservations
            var laundryItems = await _context.LaundrySignups
                .Where(l => l.DateTimeSignedUp > startOfFeedCst && l.DateTimeSignedUp <= nowCst)
                .ToListAsync();
            var laundryGroups = laundryItems.GroupBy(s => s.DateTimeSignedUp.Date);
            foreach (var g in laundryGroups)
            {
                var count = g.Count();
                if (count < 4)
                {
                    model.AddRange(g.Select(l => new FeedItem
                    {
                        UserName = l.User.UserName,
                        Name = l.User.ToString(),
                        ImageName = l.User.GetAvatarString(),
                        TimeSince = (nowCst - l.DateTimeSignedUp).ToUserFriendlyString(),
                        OccurredOn = l.DateTimeSignedUp.FromCstToUtc(),
                        DisplayText = "Laundry reservation added for " + l.DateTimeShift.ToString("f"),
                        Link = $"/laundry/schedule",
                        Symbol = "fa-tint"
                    }));
                }
                else
                {
                    model.Add(new FeedItem
                    {
                        UserName = null,
                        Name = null,
                        ImageName = "NoAvatar.jpg",
                        TimeSince = (nowUtc - g.OrderBy(l => l.DateTimeSignedUp).Last().DateTimeSignedUp).ToUserFriendlyString(),
                        OccurredOn = g.First().DateTimeSignedUp,
                        DisplayText = count + " laundry signups",
                        Link = $"/laundry/schedule",
                        Symbol = "fa-tint"
                    });
                }
            }

            // Sober shifts added
            var soberSlotsAdded = await _context.SoberSignups
                .Where(s => s.CreatedOn > startOfFeedUtc && s.CreatedOn <= nowUtc)
                .ToListAsync();
            var saas = await _context.Leaders
                .Where(l => l.Position.Name == "Sergeant-at-Arms" && l.SemesterId == thisSemester.Id).ToListAsync();
            var saa = (saas.LastOrDefault() ?? new Leader
            {
                User = await _userManager.GetUserAsync(HttpContext.User)
            }).User;
            var soberSlotGroups = soberSlotsAdded
                .Where(s => s.CreatedOn != null)
                .GroupBy(s => (s.CreatedOn.Value).Date);
            foreach (var g in soberSlotGroups)
            {
                var count = g.Count();
                if (count < 4)
                {
                    model.AddRange(g.Where(s => s.CreatedOn != null).Select(s => new FeedItem
                    {
                        UserName = saa.UserName,
                        Name = saa.ToString(),
                        ImageName = saa.GetAvatarString(),
                        TimeSince = (nowUtc - (DateTime)s.CreatedOn).ToUserFriendlyString(),
                        OccurredOn = (DateTime)s.CreatedOn,
                        DisplayText = "Sober shift added for " + s.DateOfShift.FromUtcToCst().ToString("D"),
                        Link = "/sobers/schedule",
                        Symbol = "fa-car"
                    }));
                }
                else
                {
                    model.Add(new FeedItem
                    {
                        UserName = saa.UserName,
                        Name = saa.ToString(),
                        ImageName = saa.GetAvatarString(),
                        TimeSince = (nowUtc - (g.First().CreatedOn ?? nowUtc)).ToUserFriendlyString(),
                        OccurredOn = g.First().CreatedOn ?? nowUtc,
                        DisplayText = count + " sober shifts added",
                        Link = "/sobers/schedule",
                        Symbol = "fa-car"
                    });
                }
            }

            // Sober shifts filled
            var soberSignups = await _context.SoberSignups
                .Where(s => s.DateTimeSignedUp > startOfFeedUtc && s.DateTimeSignedUp <= nowUtc)
                .ToListAsync();
            model.AddRange(soberSignups.Where(s => s.DateTimeSignedUp != null).Select(s => new FeedItem
            {
                UserName = s.User.UserName,
                Name = s.User.ToString(),
                ImageName = s.User.GetAvatarString(),
                TimeSince = (nowUtc - (DateTime)s.DateTimeSignedUp).ToUserFriendlyString(),
                OccurredOn = (DateTime)s.DateTimeSignedUp,
                DisplayText = "New volunteer for Sober " + s.SoberType.Name + " on " + s.DateOfShift.FromUtcToCst().ToString("D"),
                Link = "/sobers/schedule",
                Symbol = "fa-beer"
            }));

            // Meal Plates
            var plates = await _context.MealPlates
                .Where(p => p.SignedUpOn > startOfFeedUtc && p.SignedUpOn <= nowUtc)
                .ToListAsync();
            var plateGroups = plates.GroupBy(s => s.SignedUpOn.Date);
            foreach (var g in plateGroups)
            {
                var count = g.Count();
                if (count < 4)
                {
                    model.AddRange(g.Select(s => new FeedItem
                    {
                        UserName = s.User.UserName,
                        Name = s.User.ToString(),
                        ImageName = s.User.GetAvatarString(),
                        TimeSince = (nowUtc - s.SignedUpOn).ToUserFriendlyString(),
                        OccurredOn = s.SignedUpOn,
                        DisplayText = s.Type + " plate signup added",
                        Link = "/kitchen/meals/schedule",
                        Symbol = "fa-cutlery"
                    }));
                }
                else
                {
                    model.Add(new FeedItem
                    {
                        UserName = null,
                        Name = null,
                        ImageName = "NoAvatar.jpg",
                        TimeSince = (nowUtc - g.OrderBy(l => l.SignedUpOn).Last().SignedUpOn).ToUserFriendlyString(),
                        OccurredOn = g.OrderBy(l => l.SignedUpOn).Last().SignedUpOn,
                        DisplayText = count + " plate signups added",
                        Link = "/kitchen/meals/schedule",
                        Symbol = "fa-cutlery"
                    });
                }
            }

            // Service
            var serviceEvents = await _context.ServiceEvents
                .Where(e => e.CreatedOn > startOfFeedUtc && e.CreatedOn <= nowUtc)
                .ToListAsync();
            model.AddRange(serviceEvents.Where(s => s.CreatedOn != null).Select(s => new FeedItem
            {
                UserName = s.Submitter.UserName,
                Name = s.Submitter.ToString(),
                ImageName = s.Submitter.GetAvatarString(),
                TimeSince = (nowUtc - (DateTime)s.CreatedOn).ToUserFriendlyString(),
                OccurredOn = (DateTime)s.CreatedOn,
                DisplayText = s.EventName + " service event was added",
                Link = $"/service/events/details?id={s.EventId}",
                Symbol = "fa-globe"
            }));

            // Scholarships
            var scholarshipSubmissions = await _context.ScholarshipSubmissions
                .Where(s => s.SubmittedOn > startOfFeedUtc && s.SubmittedOn <= nowUtc)
                .ToListAsync();
            model.AddRange(scholarshipSubmissions.Select(s => new FeedItem
            {
                UserName = string.Empty,
                Name = s.FirstName + " " + s.LastName,
                ImageName = "NoAvatar.jpg",
                TimeSince = (nowUtc - s.SubmittedOn).ToUserFriendlyString(),
                OccurredOn = s.SubmittedOn,
                DisplayText = "New scholarship application was submitted",
                Link = $"/scholarships/applications/submission?id={s.ScholarshipSubmissionId}",
                Symbol = "fa-graduation-cap"
            }));

            // Incidents
            var incidents = await _context.IncidentReports
                .Where(i => i.DateTimeSubmitted > startOfFeedUtc && i.DateTimeSubmitted <= nowUtc)
                .ToListAsync();
            model.AddRange(incidents.Select(s => new FeedItem
            {
                UserName = string.Empty,
                Name = string.Empty,
                ImageName = "NoAvatar.jpg",
                TimeSince = (nowUtc - s.DateTimeSubmitted).ToUserFriendlyString(),
                OccurredOn = s.DateTimeSubmitted,
                DisplayText = "New incident report submitted",
                Link = $"/members/incidents/details?id={s.Id}",
                Symbol = "fa-exclamation-triangle"
            }));

            // Work Orders
            var workOrders = await _context.WorkOrders
                .Where(w => w.CreatedOn > startOfFeedUtc && w.CreatedOn <= nowUtc && w.ClosedOn == null)
                .ToListAsync();
            model.AddRange(workOrders.Select(s => new FeedItem
            {
                UserName = s.User.UserName,
                Name = s.User.ToString(),
                ImageName = s.User.GetAvatarString(),
                TimeSince = (nowUtc - s.CreatedOn).ToUserFriendlyString(),
                OccurredOn = s.CreatedOn,
                DisplayText = s.Title + " work order submitted",
                Link = $"/house/workorders/view?id={s.WorkOrderId}",
                Symbol = "fa-wrench"
            }));

            return Ok(model.OrderByDescending(i => i.OccurredOn).Skip(page * pageLength).Take(pageLength).ToList());
        }
    }
}
