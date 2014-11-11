namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.UI.WebControls.WebParts;
    using App_Start;
    using Data.UnitOfWork;
    using Microsoft.AspNet.Identity;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class SphinxController : BaseController
    {
        public SphinxController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        public ActionResult Index(string message = "")
        {
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            var member = uow.MemberRepository.SingleById(userId);
            var events = GetAllCompletedEventsForUser(userId).ToList();
            var startOfTodayCst = ConvertUtcToCst(DateTime.UtcNow).Date;
            var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            var sevenDaysAheadOfToday = startOfTodayUtc.AddDays(7);

            if (startOfTodayCst.Hour < 6)
            {
                startOfTodayUtc = startOfTodayUtc.AddDays(-1);
            }

            var soberSignups = uow.SoberSignupsRepository.SelectAll()
                .Where(s =>
                    s.DateOfShift >= startOfTodayUtc &&
                    s.DateOfShift <= sevenDaysAheadOfToday)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.Type)
                .ToList();

            var thisSemester = GetThisSemester();
            var memberSoberSignups = GetSoberSignupsForUser(userId, thisSemester);

            var laundrySignups = uow.LaundrySignupRepository
                    .SelectBy(l => l.DateTimeShift >= DateTime.UtcNow)
                    .OrderBy(l => l.DateTimeShift)
                    .ToList();
            var laundryTake = laundrySignups.Count;
            if (laundrySignups.Count > 5)
            {
                laundryTake = 5;
            }

            var model = new SphinxModel
            {
                MemberInfo = member,
                Roles = Roles.GetRolesForUser(member.UserName),
                RemainingCommunityServiceHours = GetRemainingServiceHoursForUser(userId),
                RemainingStudyHours = GetRemainingStudyHoursForUser(userId),
                RemainingProctoredStudyHours = GetRemainingProctoredStudyHoursForUser(userId),
                CompletedEvents = events,
                StudyHours = GetStudyHoursForUser(userId),
                SoberSignups = soberSignups,
                LaundrySummary = laundrySignups.Take(laundryTake),
                NeedsToSoberDrive = !memberSoberSignups.Any(),
                CurrentSemester = thisSemester,
                PreviousSemester = GetLastSemester()
            };

            ViewBag.Message = message;

            return View(model);
        }

        #region Sober Scheduling

        public ActionResult SoberSchedule(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date).AddDays(-1).AddHours(3);

            var signups =
                uow.SoberSignupsRepository.SelectAll()
                    .Where(s => s.DateOfShift >= threeAmYesterday)
                    .OrderBy(s => s.DateOfShift)
                    .ToList();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult SoberScheduleManager()
        {
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var vacantSignups =
                uow.SoberSignupsRepository.SelectAll()
                    .Where(s => s.DateOfShift >= startOfTodayUtc && s.UserId == null)
                    .OrderBy(s => s.DateOfShift)
                    .ToList();

            return View(vacantSignups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult RequestSoberMember(SoberSignup signup)
        {
            if (!ModelState.IsValid) return RedirectToAction("SoberScheduleManager", "Sphinx");

            signup.DateOfShift = ConvertCstToUtc(signup.DateOfShift);

            uow.SoberSignupsRepository.Insert(signup);
            uow.Save();

            return RedirectToAction("SoberScheduleManager", "Sphinx");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult SoberSignupDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            uow.SoberSignupsRepository.DeleteById(id);
            uow.Save();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        public ActionResult SoberSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = uow.SoberSignupsRepository.SingleById(id);

            if (signup.UserId != null)
                return RedirectToAction("SoberSchedule", "Sphinx");
            if (User.IsInRole("Pledge") && signup.Type == SoberSignupType.Officer)
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = uow.MemberRepository.Single(m => m.UserName == User.Identity.Name).UserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;

            uow.SoberSignupsRepository.Update(signup);
            uow.Save();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        public async Task<ActionResult> SoberSignupCancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = uow.SoberSignupsRepository.SingleById(id);
            var oldUserId = signup.UserId;
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            if (signup.UserId == null ||
                (signup.UserId != userId &&
                !User.IsInRole("Administrator") &&
                !User.IsInRole("Sergeant-at-Arms")))
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = null;
            signup.DateTimeSignedUp = null;

            uow.SoberSignupsRepository.Update(signup);
            uow.Save();

            if (WebSecurity.GetUserId(User.Identity.Name) != oldUserId)
                return RedirectToAction("SoberSchedule", "Sphinx");

            var member = uow.MemberRepository.SingleById(oldUserId);
            var currentSemesterId = GetThisSemestersId();
            var position = uow.PositionRepository.Single(p => p.PositionName == "Sergeant-at-Arms");
            var saa = uow.LeaderRepository.Single(l => l.SemesterId == currentSemesterId && l.PositionId == position.PositionId);

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
        public ActionResult EditSoberSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var signup = uow.SoberSignupsRepository.SingleById(id);
            var model = new EditSoberSignupModel
            {
                SoberSignup = signup,
                Members = GetUserIdListAsFullNameWithNone()
            };

            if (signup.UserId != null)
                model.SelectedMember = (int)signup.UserId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult EditSoberSignup(EditSoberSignupModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("SoberSchedule", new { message = "Failed to update sober signup." });

            var existingSignup = uow.SoberSignupsRepository.SingleById(model.SoberSignup.SignupId);

            if (model.SelectedMember <= 0)
                existingSignup.UserId = null;
            else
                existingSignup.UserId = model.SelectedMember;

            uow.SoberSignupsRepository.Update(existingSignup);
            uow.Save();

            return RedirectToAction("SoberSchedule", new { message = "Successfully updated sober signup." });
        }

        #endregion

        #region Laundry Scheduling

        [HttpGet]
        public ActionResult LaundrySchedule(LaundrySignupMessage? message)
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

            var existingSignups = uow.LaundrySignupRepository.SelectBy(l => l.DateTimeShift > startOfTodayUtc).ToList();
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
        public ActionResult ReserveLaundry(LaundrySignup signup)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailed });

            var startOfToday = DateTime.UtcNow.Date;
            var currentUserId = WebSecurity.GetUserId(User.Identity.Name);
            var existingSignups = uow.LaundrySignupRepository.SelectBy(l =>
                l.DateTimeShift >= startOfToday &&
                l.UserId == currentUserId)
                .ToList();

            var maxSignups = 2;
            if (User.IsInRole("House Steward")) maxSignups = 4;

            if (existingSignups.Count() >= maxSignups)
            {
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailedTooMany });
            }

            signup.UserId = currentUserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;
            signup.DateTimeShift = ConvertCstToUtc(signup.DateTimeShift);

            uow.LaundrySignupRepository.Insert(signup);
            uow.Save();

            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveSuccess });
        }
        [HttpPost]
        public ActionResult CancelReserveLaundry(LaundrySignup cancel)
        {
            cancel.DateTimeShift = ConvertCstToUtc(cancel.DateTimeShift);
            var shift = uow.LaundrySignupRepository.Single(s => s.DateTimeShift == cancel.DateTimeShift);
            if (shift == null)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });

            uow.LaundrySignupRepository.Delete(shift);
            uow.Save();

            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });
        }

        #endregion

        #region Error Messages

        private static dynamic GetLaundrySignupMessage(LaundrySignupMessage? message)
        {
            return message == LaundrySignupMessage.ReserveSuccess ? "You have successfully reserved a time slot."
                : message == LaundrySignupMessage.ReserveFailed ? "Laundry signup has failed.  Please try again or contact the system administrator."
                : message == LaundrySignupMessage.CancelReservationSuccess ? "Your reservation has been successfully removed."
                : message == LaundrySignupMessage.CancelReservationFailed ? "Your reservation was unable to be removed.  Please try again or contact the system administrator."
                : message == LaundrySignupMessage.ReserveFailedTooMany ? "You have reserved to many slots within the coming week. Please cancel one or more before you attempt to reserve another."
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