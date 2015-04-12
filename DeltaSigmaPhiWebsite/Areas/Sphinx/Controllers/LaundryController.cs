namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Models;
    using WebMatrix.WebData;
    using Newtonsoft.Json;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class LaundryController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Schedule(LaundrySignupMessage? message)
        {
            switch (message)
            {
                case LaundrySignupMessage.ReserveSuccess:
                case LaundrySignupMessage.CancelReservationSuccess:
                    ViewBag.SuccessMessage = GetLaundrySignupMessage(message);
                    break;
                case LaundrySignupMessage.ReserveFailed:
                case LaundrySignupMessage.ReserveFailedTooMany:
                case LaundrySignupMessage.CancelReservationFailed:
                    ViewBag.FailMessage = GetLaundrySignupMessage(message);
                    break;
            }
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            const int hoursInDay = 24;
            const int slotSize = 2;

            var existingSignups = await _db.LaundrySignups.Where(l => l.DateTimeShift >= startOfTodayUtc).ToListAsync();
            var slots = new List<List<LaundrySignup>>();

            for (var y = 0; y < 12; y++) // Hours in day
            {
                var timeSlotSchedule = new List<LaundrySignup>();
                for (var x = 0; x < 7; x++) // Days of week
                {
                    var timeSlot = startOfTodayUtc.AddHours(y * slotSize + x * hoursInDay);
                    if (existingSignups.Any(s => s.DateTimeShift == timeSlot))
                    {
                        var existing = existingSignups.Single(s => s.DateTimeShift == timeSlot);
                        var reservation = new LaundrySignup
                        {
                            DateTimeSignedUp = existing.DateTimeSignedUp,
                            DateTimeShift = ConvertUtcToCst(timeSlot),
                            Member = existing.Member,
                            UserId = existing.UserId
                        };
                        timeSlotSchedule.Add(reservation);
                    }
                    else
                    {
                        timeSlotSchedule.Add(new LaundrySignup { DateTimeShift = ConvertUtcToCst(timeSlot) });
                    }
                }
                slots.Add(timeSlotSchedule);
            }

            // Get semester list
            var signups = await _db.LaundrySignups.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = new List<Semester>();
            foreach (var s in allSemesters)
            {
                if (signups.Any(i => i.DateTimeShift >= s.DateStart && i.DateTimeShift <= s.DateEnd))
                {
                    semesters.Add(s);
                }
            }

            var model = new LaundryIndexModel
            {
                Slots = slots,
                SemesterList = await GetCustomSemesterListAsync(semesters)
            };
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Reserve(LaundrySignup signup)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Schedule", new { Message = LaundrySignupMessage.ReserveFailed });

            var startOfToday = DateTime.UtcNow.Date;
            var currentUserId = WebSecurity.GetUserId(User.Identity.Name);
            var existingSignups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= startOfToday &&
                            l.UserId == currentUserId)
                .ToListAsync();

            var maxSignups = 2;
            if (User.IsInRole("House Steward")) maxSignups = 4;

            if (existingSignups.Count() >= maxSignups)
            {
                return RedirectToAction("Schedule", new { Message = LaundrySignupMessage.ReserveFailedTooMany });
            }

            signup.UserId = currentUserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;
            signup.DateTimeShift = ConvertCstToUtc(signup.DateTimeShift);

            _db.LaundrySignups.Add(signup);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { Message = LaundrySignupMessage.ReserveSuccess });
        }
        [HttpPost]
        public async Task<ActionResult> Cancel(LaundrySignup cancel)
        {
            cancel.DateTimeShift = ConvertCstToUtc(cancel.DateTimeShift);
            var shift = await _db.LaundrySignups.SingleAsync(s => s.DateTimeShift == cancel.DateTimeShift);

            if (shift == null)
                return RedirectToAction("Schedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });

            _db.LaundrySignups.Remove(shift);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });
        }
        [HttpGet]
        public async Task<ActionResult> GetSignups(int? sid)
        {
            var semester = await _db.Semesters.FindAsync(sid);
            if (semester == null)
                semester = await base.GetThisSemesterAsync();

            var signups = await _db.LaundrySignups
                .Where(l => l.DateTimeShift >= semester.DateStart && l.DateTimeShift <= semester.DateEnd)
                .OrderBy(l => l.DateTimeShift)
                .ToListAsync();

            var data = signups.Select(l => new
                {
                    DateTimeShift = base.ConvertUtcToCst(l.DateTimeShift),
                    DateTimeSignedUp = base.ConvertUtcToCst(l.DateTimeSignedUp),
                    l.UserId
                }).ToList();

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        private static dynamic GetLaundrySignupMessage(LaundrySignupMessage? message)
        {
            return message == LaundrySignupMessage.ReserveSuccess ? "Reservation succeeded."
                : message == LaundrySignupMessage.ReserveFailed ? "Reservation failed.  Please try again or contact the system administrator."
                : message == LaundrySignupMessage.CancelReservationSuccess ? "Reservation cancellation succeeded."
                : message == LaundrySignupMessage.CancelReservationFailed ? "Reservation cancellation failed.  Please try again or contact the system administrator."
                : message == LaundrySignupMessage.ReserveFailedTooMany ? "Reservation failed. You have reached the maximum number of allowable reservations for this time period."
                : "";
        }

        public enum LaundrySignupMessage
        {
            ReserveSuccess,
            CancelReservationSuccess,
            ReserveFailed,
            ReserveFailedTooMany,
            CancelReservationFailed
        }
    }
}