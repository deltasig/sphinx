namespace Dsp.Areas.Sphinx.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class LaundryController : BaseController
    {
        [HttpGet, Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Schedule()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            // Build Laundry Schedule
            var nowCst = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central Standard Time");
            var existingSignups = await _db.LaundrySignups.Where(l => l.DateTimeShift >= nowCst.Date).ToListAsync();
            var schedule = new LaundrySchedule(nowCst, 7, 2, existingSignups);

            // Get semester list for stats.
            var signups = await _db.LaundrySignups.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var thisSemester = await GetThisSemesterAsync();
            var semesters = allSemesters
                .Where(s => 
                    signups.Any(i => 
                        i.DateTimeShift >= s.DateStart && 
                        i.DateTimeShift <= s.DateEnd))
                .ToList();
            // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
            if (semesters.All(s => s.SemesterId != thisSemester.SemesterId))
            {
                semesters.Add(thisSemester);
            }

            var model = new LaundryIndexModel
            {
                Schedule = schedule,
                SemesterList = GetCustomSemesterListAsync(semesters)
            };
            return View(model);
        }

        [HttpPost, Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Reserve(LaundrySignup signup)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "There was an unknown error with your reservation.  " +
                                             "Contact your administrator if the problem persists.";
                return RedirectToAction("Schedule");
            }

            var nowCst = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central Standard Time");
            var currentUserId = User.Identity.GetUserId<int>();

            // Check if they've already signed up too many times within the current window.
            var existingSignups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= nowCst.Date && l.UserId == currentUserId).ToListAsync();
            var maxSignups = 2;
            if (User.IsInRole("House Steward")) maxSignups = 4;
            if (existingSignups.Count() >= maxSignups)
            {
                TempData["FailureMessage"] = "You have signed up too many times within the current window.  " +
                                             "Your maximum allowed is: " + maxSignups;
                return RedirectToAction("Schedule");
            }

            // Check if a signup already exists
            var shift = await _db.LaundrySignups.SingleOrDefaultAsync(s => s.DateTimeShift == signup.DateTimeShift);
            if (shift != null)
            {
                TempData["FailureMessage"] = "Sorry, " + shift.Member + " signed up for that slot after you loaded page " +
                                             "but before you tried to sign up.  You will have to pick a new slot.";
                return RedirectToAction("Schedule");
            }

            // All good, add their reservation.
            signup.UserId = currentUserId;
            signup.DateTimeSignedUp = nowCst;
            signup.DateTimeShift = signup.DateTimeShift;

            _db.LaundrySignups.Add(signup);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "You have reserved the laundry room for the following time: " + signup.DateTimeShift;
            return RedirectToAction("Schedule");
        }

        [HttpPost, Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Cancel(LaundrySignup cancel)
        {
            var shift = await _db.LaundrySignups.SingleOrDefaultAsync(s => s.DateTimeShift == cancel.DateTimeShift);

            if (shift == null)
            {
                TempData["FailureMessage"] = "Could not cancel reservation because no existing reservation was found.";
                return RedirectToAction("Schedule");
            }
            if (shift.UserId != User.Identity.GetUserId<int>())
            {
                TempData["FailureMessage"] = "You cannot cancel someone else's shift!";
                return RedirectToAction("Schedule");
            }

            _db.LaundrySignups.Remove(shift);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "You cancelled your reservation for: " + shift.DateTimeShift;
            return RedirectToAction("Schedule");
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> GetStats(int? sid)
        {
            var model = new LaundryStatsModel();
            var semester = await _db.Semesters.FindAsync(sid) ?? await GetThisSemesterAsync();
            var signups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= semester.DateStart && l.DateTimeShift <= semester.DateEnd)
                .OrderBy(l => l.DateTimeShift)
                .ToListAsync();

            var formattedSignups = signups.Select(l => new
            {
                DateTimeShift = ConvertUtcToCst(l.DateTimeShift),
                DateTimeSignedUp = ConvertUtcToCst(l.DateTimeSignedUp),
                l.UserId
            }).ToList();

            // Build model
            model.TotalSignups = formattedSignups.Count();
            model.Semester = semester.ToString();
            model.StartDate = ConvertUtcToCst(semester.DateStart).ToShortDateString();
            model.EndDate = ConvertUtcToCst(semester.DateEnd).ToShortDateString();

            // Month Calculations
            var months = MonthsBetween(semester.DateStart, semester.DateEnd).OrderBy(m => m.Month).ToList();
            var monthValues = months.Select(m => m.Month).Distinct();
            model.MonthChartXLabels = months.Select(m => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month)).ToList();
            model.MonthChartXValues = new List<int>();
            foreach (var m in monthValues)
            {
                model.MonthChartXValues.Add(signups.Count(s => s.DateTimeShift.Month == m));
            }
            model.MonthAverage = (decimal)model.TotalSignups / months.Count;

            // Week Calculations
            var startWeek = GetIso8601WeekOfYear(semester.DateStart);
            var endWeek = GetIso8601WeekOfYear(semester.DateEnd);
            var totalWeeks = endWeek - startWeek;
            model.WeekChartXLabels = new List<string>(GetDayNames());
            model.WeekChartXValues = new List<decimal>();
            for (var w = 0; w <= 6; w++)
            {
                var averageSignups = (decimal)signups.Count(s => (int)s.DateTimeShift.DayOfWeek == w) / totalWeeks;
                model.WeekChartXValues.Add(decimal.Round(averageSignups, 2, MidpointRounding.AwayFromZero));
            }
            model.WeekAverage = decimal.Round((decimal)model.TotalSignups / totalWeeks, 2, MidpointRounding.AwayFromZero);

            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> MemberUsage()
        {
            // Get semester list
            var signups = await _db.LaundrySignups.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var thisSemester = await GetThisSemesterAsync();
            var semesters = allSemesters
                .Where(s => 
                    signups.Any(i => 
                        i.DateTimeShift >= s.DateStart && 
                        i.DateTimeShift <= s.DateEnd))
                .ToList();
            // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
            if (semesters.All(s => s.SemesterId != thisSemester.SemesterId))
            {
                semesters.Add(thisSemester);
            }

            return View(GetCustomSemesterListAsync(semesters));
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult> GetUsageStats(int? sid)
        {
            var model = new LaundryUsageStatsModel();

            var semester = await _db.Semesters.FindAsync(sid) ?? await GetThisSemesterAsync();

            var signups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= semester.DateStart && l.DateTimeShift <= semester.DateEnd)
                .OrderBy(l => l.DateTimeShift)
                .ToListAsync();

            var formattedSignups = signups.Select(l => new
            {
                DateTimeShift = ConvertUtcToCst(l.DateTimeShift),
                DateTimeSignedUp = ConvertUtcToCst(l.DateTimeSignedUp),
                l.UserId
            }).ToList();

            // Build model
            model.TotalSignups = formattedSignups.Count();
            model.TotalMembers = formattedSignups.Select(s => s.UserId).Distinct().Count();
            model.Semester = semester.ToString();
            model.StartDate = ConvertUtcToCst(semester.DateStart).ToShortDateString();
            model.EndDate = ConvertUtcToCst(semester.DateEnd).ToShortDateString();

            // Calculations
            var values = formattedSignups
                .GroupBy(s => s.UserId)
                .Select(s => new { UserId = s.Key, Value = s.Count() })
                .OrderByDescending(s => s.Value)
                .ToList();
            model.ChartXValues = values.Select(s => s.Value).ToList();
            model.ChartXLabels = new List<string>();
            for (var i = 0; i < values.Count; i++)
            {
                if (signups.First(s => s.UserId == values[i].UserId).Member.WasLivingInHouse(semester.DateEnd))
                {
                    model.ChartXLabels.Add((i+1) + "*");
                }
                else
                {
                    model.ChartXLabels.Add((i+1).ToString());
                }
            }

            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private static IEnumerable<string> GetDayNames()
        {
            if (CultureInfo.CurrentCulture.Name.StartsWith("en-"))
            {
                return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            }
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
        }

        private static IEnumerable<DateTime> MonthsBetween(DateTime d0, DateTime d1)
        {
            return Enumerable.Range(0, (d1.Year - d0.Year) * 12 + (d1.Month - d0.Month + 1))
                             .Select(m => new DateTime(d0.Year, d0.Month, 1).AddMonths(m));
        }

        private static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        } 
    }
}