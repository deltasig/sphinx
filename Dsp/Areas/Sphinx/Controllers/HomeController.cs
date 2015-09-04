﻿namespace Dsp.Areas.Sphinx.Controllers
{
    using Dsp.Controllers;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class HomeController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string userName, string message = "")
        {
            ViewBag.Message = message;
            
            if (!User.IsInRole("Administrator") || string.IsNullOrEmpty(userName))
            {
                userName = User.Identity.Name;
            }

            var twoHoursAgoCst = ConvertUtcToCst(DateTime.UtcNow).AddHours(-2);
            var member = await UserManager.FindByNameAsync(userName);
            var events = await GetAllCompletedEventsForUserAsync(member.Id);
            var thisSemester = await GetThisSemesterAsync();

            var thisWeeksSoberShifts = await GetThisWeeksSoberSignupsAsync(DateTime.UtcNow);
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
                PreviousSemester = await GetLastSemesterAsync(),
            };

            return View(model);
        }

        public async Task<ActionResult> Feed(int page = 0)
        {
            const int pageLength = 10;
            const int feedLengthDays = 30;
            // Limit page minimum and maximum to [0, 10] (Can't go more than 10 weeks back if pageLength equals 7)
            page = page > 10 ? 10 : page;
            page = page < 0 ? 0 : page;
            var nowUtc = DateTime.UtcNow;
            var nowCst = ConvertUtcToCst(nowUtc);
            var startOfFeedUtc = nowUtc.AddDays(-feedLengthDays);
            var startOfFeedCst = nowCst.AddDays(-feedLengthDays);
            var thisSemester = await GetThisSemesterAsync();
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
                DaysSince = (nowCst - l.DateTimeSignedUp).Days,
                OccurredOn = l.DateTimeSignedUp,
                DisplayText = "Laundry reservation added for " + l.DateTimeShift.ToString("f"),
                Link = Url.Action("Schedule", "Laundry"),
                Symbol = "fa-tint"
            }));

            // Sober shifts added
            var soberSlotsAdded = await _db.SoberSignups
                .Where(s => s.CreatedOn > startOfFeedUtc && s.CreatedOn <= nowUtc)
                .ToListAsync();
            var saas = await _db.Leaders
                .Where(l => l.Position.Name == "Sergeant-at-Arms" && l.SemesterId == thisSemester.SemesterId).ToListAsync();
            var saa = (saas.LastOrDefault() ?? new Entities.Leader
            {
                Member = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>())
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
                        DaysSince = (nowUtc - (DateTime) s.CreatedOn).Days,
                        OccurredOn = (DateTime) s.CreatedOn,
                        DisplayText = "Sober shift added for " + ConvertUtcToCst(s.DateOfShift).ToString("D"),
                        Link = Url.Action("Schedule", "Sobers"),
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
                        DaysSince = (nowUtc - (g.First().CreatedOn ?? nowUtc)).Days,
                        OccurredOn = (g.First().CreatedOn ?? nowUtc),
                        DisplayText = count + " sober shifts added",
                        Link = Url.Action("Schedule", "Sobers"),
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
                DaysSince = (nowUtc - (DateTime)s.DateTimeSignedUp).Days,
                OccurredOn = (DateTime)s.DateTimeSignedUp,
                DisplayText = "New volunteer for Sober " + s.SoberType.Name + " on " + ConvertUtcToCst(s.DateOfShift).ToString("D"),
                Link = Url.Action("Schedule", "Sobers"),
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
                    DaysSince = (nowUtc - s.SignedUpOn).Days,
                    OccurredOn = s.SignedUpOn,
                    DisplayText = s.Type + " plate signup added",
                    Link = Url.Action("Schedule", "Meals", new { area = "Kitchen" }),
                    Symbol = "fa-cutlery"
                }));
            }

            // Service
            var serviceEvents = await _db.Events
                .Where(e => e.CreatedOn > startOfFeedUtc && e.CreatedOn <= nowUtc)
                .ToListAsync();
            model.AddRange(serviceEvents.Where(s => s.CreatedOn != null).Select(s => new FeedItem
            {
                UserName = s.Submitter.UserName,
                Name = s.Submitter.ToString(),
                ImageName = s.Submitter.GetAvatarString(),
                DaysSince = (nowUtc - (DateTime)s.CreatedOn).Days,
                OccurredOn = (DateTime)s.CreatedOn,
                DisplayText = s.EventName + " service event was added",
                Link = Url.Action("Details", "Events", new { area = "Service", id = s.EventId }),
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
                DaysSince = (nowUtc - s.SubmittedOn).Days,
                OccurredOn = s.SubmittedOn,
                DisplayText = "New scholarship application was submitted",
                Link = Url.Action("Submission", "Applications", new { area = "Scholarships", id = s.ScholarshipSubmissionId }),
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
                DaysSince = (nowUtc - s.UploadedOn).Days,
                OccurredOn = s.UploadedOn,
                DisplayText = "New file uploaded for " + s.Class.CourseShorthand,
                Link = Url.Action("Details", "Classes", new { area = "Edu", id = s.ClassId }),
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
                DaysSince = (nowUtc - s.DateTimeSubmitted).Days,
                OccurredOn = s.DateTimeSubmitted,
                DisplayText = "New incident report submitted",
                Link = Url.Action("Details", "Incidents", new { area = "Members", id = s.IncidentId }),
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
                DaysSince = (nowUtc - s.ChangedOn).Days,
                OccurredOn = s.ChangedOn,
                DisplayText = s.WorkOrder.Title + " work order submitted",
                Link = Url.Action("View", "WorkOrders", new { area = "House", id = s.WorkOrderId }),
                Symbol = "fa-wrench"
            }));

            return PartialView("Feed", model.OrderByDescending(i => i.OccurredOn).Skip(page*pageLength).Take(pageLength).ToList());
        }
    }
}