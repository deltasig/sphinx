namespace DeltaSigmaPhiWebsite.Controllers
{
    using App_Start;
    using Microsoft.AspNet.Identity;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class SphinxController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string message = "")
        {
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            var member = await _db.Members.FindAsync(userId);
            var events = await GetAllCompletedEventsForUserAsync(userId);
            var startOfTodayCst = ConvertUtcToCst(DateTime.UtcNow).Date;
            var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            var sevenDaysAheadOfToday = startOfTodayUtc.AddDays(7);

            if (startOfTodayCst.Hour < 6)
            {
                startOfTodayUtc = startOfTodayUtc.AddDays(-1);
            }

            var soberSignups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= startOfTodayUtc && 
                            s.DateOfShift <= sevenDaysAheadOfToday)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.Type)
                .ToListAsync();

            var thisSemester = await GetThisSemesterAsync();
            var memberSoberSignups = await GetSoberSignupsForUserAsync(userId, thisSemester);

            var laundrySignups = await _db.LaundrySignups
                    .Where(l => l.DateTimeShift >= DateTime.UtcNow)
                    .OrderBy(l => l.DateTimeShift)
                    .ToListAsync();
            var laundryTake = laundrySignups.Count;
            if (laundrySignups.Count > 5)
            {
                laundryTake = 5;
            }

            var model = new SphinxModel
            {
                MemberInfo = member,
                Roles = Roles.GetRolesForUser(member.UserName),
                RemainingCommunityServiceHours = await GetRemainingServiceHoursForUserAsync(userId),
                RemainingStudyHours = await GetRemainingStudyHoursForUserAsync(userId),
                RemainingProctoredStudyHours = await GetRemainingProctoredStudyHoursForUserAsync(userId),
                CompletedEvents = events,
                StudyHours = await GetStudyHoursForUserAsync(userId),
                SoberSignups = soberSignups,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = await  GetLastSemesterAsync()
            };

            ViewBag.Message = message;

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> AlumniLeaders()
        {
            var currentSemester = await GetThisSemesterAsync();

            if (currentSemester == null) return View();

            var model = await _db.Leaders
                .Where(l => l.SemesterId == currentSemester.SemesterId && 
                            l.Position.Type == Position.PositionType.Alumni)
                .ToListAsync();

            return View(model);
        }

        #region Sober Scheduling

        public async Task<ActionResult> SoberSchedule(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date).AddDays(-1).AddHours(3);

            var signups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= threeAmYesterday)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> SoberScheduleManager()
        {
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var vacantSignups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= startOfTodayUtc && 
                            s.UserId == null)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();

            return View(vacantSignups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> RequestSoberMember(SoberSignup signup)
        {
            if (!ModelState.IsValid) return RedirectToAction("SoberScheduleManager", "Sphinx");

            signup.DateOfShift = ConvertCstToUtc(signup.DateOfShift);

            _db.SoberSchedule.Add(signup);
            await _db.SaveChangesAsync();

            return RedirectToAction("SoberScheduleManager", "Sphinx");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> SoberSignupDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signupToCancel = await _db.SoberSchedule.FindAsync(id);
            _db.SoberSchedule.Remove(signupToCancel);
            await _db.SaveChangesAsync();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        public async Task<ActionResult> SoberSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSchedule.FindAsync(id);

            if (signup.UserId != null)
                return RedirectToAction("SoberSchedule", "Sphinx");
            if (User.IsInRole("Pledge") && signup.Type == SoberSignupType.Officer)
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = (await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name)).UserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;

            _db.Entry(signup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        public async Task<ActionResult> SoberSignupCancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSchedule.FindAsync(id);
            var oldUserId = signup.UserId;
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            if (signup.UserId == null ||
                (signup.UserId != userId &&
                !User.IsInRole("Administrator") &&
                !User.IsInRole("Sergeant-at-Arms")))
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = null;
            signup.DateTimeSignedUp = null;

            _db.Entry(signup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            if (WebSecurity.GetUserId(User.Identity.Name) != oldUserId)
                return RedirectToAction("SoberSchedule", "Sphinx");

            var member = await _db.Members.FindAsync(oldUserId);
            var currentSemesterId = await GetThisSemestersIdAsync();
            var position = await _db.Positions.SingleAsync(p => p.PositionName == "Sergeant-at-Arms");
            var saa = await _db.Leaders.SingleAsync(l => l.SemesterId == currentSemesterId && l.PositionId == position.PositionId);

            var message = new IdentityMessage
            {
                Subject = "Sphinx - Sober Signup Cancellation: " + member.ToString(),
                Body = member + " has cancelled his signup for " + signup.DateOfShift.ToShortDateString() + ".",
                Destination = saa.Member.Email
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {

            }

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSoberSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var signup = await _db.SoberSchedule.FindAsync(id);
            var model = new EditSoberSignupModel
            {
                SoberSignup = signup,
                Members = await GetUserIdListAsFullNameWithNoneAsync()
            };

            if (signup.UserId != null)
                model.SelectedMember = (int)signup.UserId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSoberSignup(EditSoberSignupModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("SoberSchedule", new { message = "Failed to update sober signup." });

            var existingSignup = await _db.SoberSchedule.FindAsync(model.SoberSignup.SignupId);

            if (model.SelectedMember <= 0)
                existingSignup.UserId = null;
            else
                existingSignup.UserId = model.SelectedMember;

            _db.Entry(existingSignup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("SoberSchedule", new { message = "Successfully updated sober signup." });
        }

        #endregion

        #region Laundry Scheduling

        [HttpGet]
        public async Task<ActionResult> LaundrySchedule(LaundrySignupMessage? message)
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

            var existingSignups = await _db.LaundrySignups.Where(l => l.DateTimeShift > startOfTodayUtc).ToListAsync();
            var model = new List<List<LaundrySignup>>();

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
                model.Add(timeSlotSchedule);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> ReserveLaundry(LaundrySignup signup)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailed });

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
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailedTooMany });
            }

            signup.UserId = currentUserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;
            signup.DateTimeShift = ConvertCstToUtc(signup.DateTimeShift);

            _db.LaundrySignups.Add(signup);
            await _db.SaveChangesAsync();

            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveSuccess });
        }
        [HttpPost]
        public async Task<ActionResult> CancelReserveLaundry(LaundrySignup cancel)
        {
            cancel.DateTimeShift = ConvertCstToUtc(cancel.DateTimeShift);
            var shift = await _db.LaundrySignups.SingleAsync(s => s.DateTimeShift == cancel.DateTimeShift);

            if (shift == null)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });

            _db.LaundrySignups.Remove(shift);
            await _db.SaveChangesAsync();

            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });
        }

        #endregion

        #region Error Messages

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

        #endregion
    }
}