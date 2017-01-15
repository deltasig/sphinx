﻿namespace Dsp.Web.Api
{
    using Data;
    using Data.Entities;
    using Extensions;
    using Services.Admin;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Areas.Sphinx.Models;
    using Microsoft.AspNet.Identity;

    [Authorize]
    [RoutePrefix("api/members")]
    public class MembersController : ApiController
    {
        private SphinxDbContext _db;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private IPositionService _positionService;

        public MembersController()
        {
            _db = new SphinxDbContext();
            _semesterService = new SemesterService(_db);
            _memberService = new MemberService(_db);
            _positionService = new PositionService(_db);
        }

        [Authorize(Roles = "Pledge, Active, Alumnus")]
        [HttpGet, Route("roster/{sid:int}")]
        public async Task<IHttpActionResult> Roster(int sid)
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

        [Authorize(Roles = "Pledge, Active, Alumnus")]
        [HttpGet, Route("alumni/{sid:int}")]
        public async Task<IHttpActionResult> Alumni(int sid)
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

        [Authorize(Roles = "Pledge, Active, Alumnus")]
        [HttpGet, Route("feed/{page:int}")]
        public async Task<IHttpActionResult> Feed(int page = 0)
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
            var laundryItems = await _db.LaundrySignups
                .Where(l => l.DateTimeSignedUp > startOfFeedCst && l.DateTimeSignedUp <= nowCst)
                .ToListAsync();
            model.AddRange(laundryItems.Select(l => new FeedItem
            {
                UserName = l.Member.UserName,
                Name = l.Member.ToString(),
                ImageName = l.Member.GetAvatarString(),
                TimeSince = (nowCst - l.DateTimeSignedUp).ToUserFriendlyString(),
                OccurredOn = l.DateTimeSignedUp.FromCstToUtc(),
                DisplayText = "Laundry reservation added for " + l.DateTimeShift.ToString("f"),
                Link = Url.Link("Default", new { area = "Sphinx", controller = "Laundry", action = "Schedule" }),
                Symbol = "fa-tint"
            }));

            // Sober shifts added
            var soberSlotsAdded = await _db.SoberSignups
                .Where(s => s.CreatedOn > startOfFeedUtc && s.CreatedOn <= nowUtc)
                .ToListAsync();
            var saas = await _db.Leaders
                .Where(l => l.Position.Name == "Sergeant-at-Arms" && l.SemesterId == thisSemester.SemesterId).ToListAsync();
            var saa = (saas.LastOrDefault() ?? new Leader
            {
                Member = await _memberService.GetMemberByIdAsync(User.Identity.GetUserId<int>())
            }).Member;
            var soberSlotGroups = soberSlotsAdded
                .Where(s => s.CreatedOn != null)
                .GroupBy(s => ((DateTime)s.CreatedOn).Date);
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
                        Link = Url.Link("Default", new { area = "Sphinx", controller = "Sobers", action = "Schedule" }),
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
                        OccurredOn = (g.First().CreatedOn ?? nowUtc),
                        DisplayText = count + " sober shifts added",
                        Link = Url.Link("Default", new { area = "Sphinx", controller = "Sobers", action = "Schedule" }),
                        Symbol = "fa-car"
                    });
                }
            }

            // Sober shifts filled
            var soberSignups = await _db.SoberSignups
                .Where(s => s.DateTimeSignedUp > startOfFeedUtc && s.DateTimeSignedUp <= nowUtc)
                .ToListAsync();
            model.AddRange(soberSignups.Where(s => s.DateTimeSignedUp != null).Select(s => new FeedItem
            {
                UserName = s.Member.UserName,
                Name = s.Member.ToString(),
                ImageName = s.Member.GetAvatarString(),
                TimeSince = (nowUtc - (DateTime)s.DateTimeSignedUp).ToUserFriendlyString(),
                OccurredOn = (DateTime)s.DateTimeSignedUp,
                DisplayText = "New volunteer for Sober " + s.SoberType.Name + " on " + s.DateOfShift.FromUtcToCst().ToString("D"),
                Link = Url.Link("Default", new { area = "Sphinx", controller = "Sobers", action = "Schedule" }),
                Symbol = "fa-beer"
            }));

            if (User.IsInRole("Administrator") || User.IsInRole("House Steward"))
            {
                // Meal Plates
                var plates = await _db.MealPlates
                    .Where(p => p.SignedUpOn > startOfFeedUtc && p.SignedUpOn <= nowUtc)
                    .ToListAsync();
                model.AddRange(plates.Select(s => new FeedItem
                {
                    UserName = s.Member.UserName,
                    Name = s.Member.ToString(),
                    ImageName = s.Member.GetAvatarString(),
                    TimeSince = (nowUtc - s.SignedUpOn).ToUserFriendlyString(),
                    OccurredOn = s.SignedUpOn,
                    DisplayText = s.Type + " plate signup added",
                    Link = Url.Link("Default", new { area = "Kitchen", controller = "Meals", action = "Schedule" }),
                    Symbol = "fa-cutlery"
                }));
            }

            // Service
            var serviceEvents = await _db.ServiceEvents
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
                Link = Url.Link("Default", new { area = "Service", controller = "Events", action = "Details", id = s.EventId }),
                Symbol = "fa-globe"
            }));

            // Scholarships
            var scholarshipSubmissions = await _db.ScholarshipSubmissions
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
                Link = Url.Link("Default", new { area = "Scholarships", controller = "Applications", action = "Submission", id = s.ScholarshipSubmissionId }),
                Symbol = "fa-graduation-cap"
            }));

            // Class Files
            var classFiles = await _db.ClassFiles
                .Where(f => f.UploadedOn > startOfFeedUtc && f.UploadedOn <= nowUtc)
                .ToListAsync();
            model.AddRange(classFiles.Select(s => new FeedItem
            {
                UserName = s.Uploader.UserName,
                Name = s.Uploader.ToString(),
                ImageName = s.Uploader.GetAvatarString(),
                TimeSince = (nowUtc - s.UploadedOn).ToUserFriendlyString(),
                OccurredOn = s.UploadedOn,
                DisplayText = "New file uploaded for " + s.Class.CourseShorthand,
                Link = Url.Link("Default", new { area = "Edu", controller = "Classes", action = "Details", id = s.ClassId }),
                Symbol = "fa-folder"
            }));

            // Incidents
            var incidents = await _db.IncidentReports
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
                Link = Url.Link("Default", new { area = "Members", controller = "Incidents", action = "Details", id = s.IncidentId }),
                Symbol = "fa-exclamation-triangle"
            }));

            // Work Orders
            var workOrders = await _db.WorkOrderStatusChanges
                .Where(w => w.ChangedOn > startOfFeedUtc && w.ChangedOn <= nowUtc && w.Status.Name == "Unread")
                .ToListAsync();
            model.AddRange(workOrders.Select(s => new FeedItem
            {
                UserName = s.WorkOrder.Member.UserName,
                Name = s.WorkOrder.Member.ToString(),
                ImageName = s.WorkOrder.Member.GetAvatarString(),
                TimeSince = (nowUtc - s.ChangedOn).ToUserFriendlyString(),
                OccurredOn = s.ChangedOn,
                DisplayText = s.WorkOrder.Title + " work order submitted",
                Link = Url.Link("Default", new { area = "House", controller = "WorkOrders", action = "View", id = s.WorkOrderId }),
                Symbol = "fa-wrench"
            }));

            return Ok(model.OrderByDescending(i => i.OccurredOn).Skip(page * pageLength).Take(pageLength).ToList());
        }
    }
}
