namespace Dsp.Web.Areas.Laundry.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class ScheduleController : BaseController
    {
        [HttpGet, Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Index()
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
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }

            // Check if a signup already exists
            var shift = await _db.LaundrySignups.SingleOrDefaultAsync(s => s.DateTimeShift == signup.DateTimeShift);
            if (shift != null)
            {
                TempData["FailureMessage"] = "Sorry, " + shift.Member + " signed up for that slot after you loaded page " +
                                             "but before you tried to sign up.  You will have to pick a new slot.";
                return RedirectToAction("Index");
            }

            // All good, add their reservation.
            signup.UserId = currentUserId;
            signup.DateTimeSignedUp = nowCst;
            signup.DateTimeShift = signup.DateTimeShift;

            _db.LaundrySignups.Add(signup);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "You have reserved the laundry room for the following time: " + signup.DateTimeShift;
            return RedirectToAction("Index");
        }

        [HttpPost, Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Cancel(LaundrySignup cancel)
        {
            var shift = await _db.LaundrySignups.SingleOrDefaultAsync(s => s.DateTimeShift == cancel.DateTimeShift);

            if (shift == null)
            {
                TempData["FailureMessage"] = "Could not cancel reservation because no existing reservation was found.";
                return RedirectToAction("Index");
            }
            if (shift.UserId != User.Identity.GetUserId<int>())
            {
                TempData["FailureMessage"] = "You cannot cancel someone else's shift!";
                return RedirectToAction("Index");
            }

            _db.LaundrySignups.Remove(shift);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "You cancelled your reservation for: " + shift.DateTimeShift;
            return RedirectToAction("Index");
        }
    }
}