namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Data.Entity;
    using System.Net;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class SphinxController : BaseController
    {
        public SphinxController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        public ActionResult Index()
        {
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            var member = uow.MemberRepository.GetById(userId);
            var events = GetAllCompletedEventsForUser(userId).ToList();

            var soberSignups = uow.SoberSignupsRepository.GetAll()
                .Where(s => DbFunctions.DiffDays(s.DateOfShift, DateTime.Now) <= 7).ToList();
            var thisSemester = GetThisSemester();
            var memberSoberSignups = GetSoberSignupsForUser(userId, thisSemester);

            var enumerable = memberSoberSignups as IList<SoberSignup> ?? memberSoberSignups.ToList();
            if (enumerable.Any())
            {
                var signup = enumerable.First(s => s.Type == SoberSignupType.Driver);
                events.Add(new ServiceHour
                {
                    DurationHours = 5,
                    Event = new Event { EventName = "Sober Driving" }
                });
            }

            var model = new SphinxModel
            {
                MemberInfo = member,
                Roles = Roles.GetRolesForUser(member.UserName),
                RemainingCommunityServiceHours = GetRemainingServiceHoursForUser(userId),
                RemainingStudyHours = GetRemainingStudyHoursForUser(userId),
                CompletedEvents = events,
                StudyModel = new StudyHourSubmissionModel
                {
                    Approvers = GetAllApproverIds(userId)
                },
                StudyHours = GetStudyHoursForUser(userId),
                StudyApproval = GetRequestedStudyHourApprovalsForUser(userId),
                ProfilePicUrl = GetPictureUrl(member.UserName),
                SoberSignups = soberSignups,
                LaundrySummary = LaundrySummary(),
                NeedsToSoberDrive = !memberSoberSignups.Any()
            };

            return View(model);
        }

        #region Academics

        [HttpGet]
        public ActionResult SubmitStudy()
        {
            var model = new StudyHourSubmissionModel
            {
                Approvers = GetAllApproverIds(WebSecurity.GetUserId(User.Identity.Name))
            };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitStudy(StudyHourSubmissionModel model)
        {
            var submission = new StudyHour
            {
                UserId = WebSecurity.GetUserId(User.Identity.Name),
                DateTimeStudied = model.DateTimeStudied,
                DateTimeSubmitted = DateTime.Now,
                DesignatedApprover = model.SelectedApproverId,
                DurationHours = model.HoursStudied
            };

            uow.StudyHourRepository.Insert(submission);
            uow.Save();
            return RedirectToAction("Index");
        }

        private int GetRemainingStudyHoursForUser(int userId)
        {
            var totalHours = 0.0;
            const double requiredHours = 20.0; //Temporary constant, eventually will be a call to db like totalHours
            var lastWeek = DateTime.Today.AddDays(-7); //This MUST be done outside the query, cannot run the AddDays function inside the query call
            try
            {
                totalHours = (from hours in uow.StudyHourRepository.GetAll()
                              where hours.UserId == userId &&
                              hours.DateTimeStudied >= lastWeek
                              select hours.DurationHours).ToList().Sum();
            }
            catch (Exception)
            {

            }

            return ((int)requiredHours - (int)totalHours);
        }
        private IEnumerable<StudyHour> GetStudyHoursForUser(int userId)
        {
            var member = uow.MemberRepository.GetById(userId);

            return member.StudyHours.Where(s => s.DateTimeStudied >= DateTime.Today.AddDays(-7));
        }
        private IEnumerable<StudyHour> GetRequestedStudyHourApprovalsForUser(int userId)
        {
            var lastWeek = DateTime.Now.AddDays(-7);
            return uow.StudyHourRepository.FindBy(s => s.DesignatedApprover == userId && s.DateTimeStudied >= lastWeek);
        }
        private IEnumerable<SelectListItem> GetAllApproverIds(int userId)
        {
            var members = uow.MemberRepository.FindBy(a => a.UserId != userId).OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }

        #endregion

        #region Incident Reporting

        [HttpGet]
        public ActionResult IncidentReporting()
        {
            return View(new IncidentReportModel
            {
                RecentIncidentReports = uow.IncidentReportRepository.GetAll(),
                Members = GetUserIdListAsFullName()
            });
        }
        [HttpPost]
        public ActionResult FileIncidentReport(IncidentReportModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("IncidentReporting", "Sphinx");

            var record = new IncidentReport
                {
                    DateTimeSubmitted = DateTime.Now,
                    ReportedBy = WebSecurity.GetUserId(User.Identity.Name),
                    DateTimeOfIncident = model.IncidentDate,
                    BehaviorsWitnessed = model.BehaviorsWitnessed ?? "",
                    PolicyBroken = model.PolicyBroken ?? ""
                };

            uow.IncidentReportRepository.Insert(record);
            uow.Save();

            return RedirectToAction("IncidentReporting", "Sphinx");
        }

        #endregion

        #region Sober Scheduling

        public ActionResult SoberSchedule()
        {
            var signups =
                uow.SoberSignupsRepository.GetAll()
                    .Where(s => s.DateOfShift >= DateTime.Now)
                    .OrderBy(s => s.DateOfShift)
                    .ToList();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult SoberScheduleManager()
        {
            var vacantSignups =
                uow.SoberSignupsRepository.GetAll()
                    .Where(s => s.DateOfShift >= DateTime.Now && s.UserId == null)
                    .OrderBy(s => s.DateOfShift)
                    .ToList();

            return View(vacantSignups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult RequestSoberMember(SoberSignup signup)
        {
            if (!ModelState.IsValid) return RedirectToAction("SoberScheduleManager", "Sphinx");

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

            var signup = uow.SoberSignupsRepository.GetById(id);

            if (signup.UserId != null)
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = uow.MemberRepository.Get(m => m.UserName == User.Identity.Name).UserId;
            uow.SoberSignupsRepository.Update(signup);
            uow.Save();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        public ActionResult SoberSignupCancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = uow.SoberSignupsRepository.GetById(id);
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            if (signup.UserId == null || signup.UserId != userId)
                return RedirectToAction("SoberSchedule", "Sphinx");

            signup.UserId = null;
            uow.SoberSignupsRepository.Update(signup);
            uow.Save();

            return RedirectToAction("SoberSchedule", "Sphinx");
        }

        #endregion

        #region Laundry Scheduling

        [HttpGet]
        public ActionResult LaundrySchedule(LaundrySignupMessage? message)
        {
            var twoHours = new TimeSpan(2, 0, 0);
            var oneDay = new TimeSpan(1, 0, 0, 0);
            var thisMorning = new DateTime((DateTime.Now.Ticks / oneDay.Ticks) * oneDay.Ticks);

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
            var signups = ThisWeeksLaundrySignup();
            var fullSchedule = new List<List<LaundryReservationModel>>();
            for (var i = 0; i < 12; i++)
            {
                var timeSlotSchedule = new List<LaundryReservationModel>();
                for (var j = 0; j < 7; j++)
                {
                    var thisTimeSlot = thisMorning + TimeSpan.FromTicks(oneDay.Ticks * j) + TimeSpan.FromTicks(twoHours.Ticks * i);
                    var reservation = signups.Find(x => x.Shift == thisTimeSlot);
                    if (reservation != null)
                    {
                        timeSlotSchedule.Add(reservation);
                    }
                    else
                    {
                        var nullReservation = new LaundryReservationModel { Name = null, Shift = thisTimeSlot };
                        timeSlotSchedule.Add(nullReservation);
                    }
                }
                fullSchedule.Add(timeSlotSchedule);
            }
            var model = new LaundrySignupModel { ThisWeeksSignups = fullSchedule };
            return View(model);
        }
        [HttpPost]
        public ActionResult ReserveLaundry(LaundryReservationModel signup)
        {
            // This verifies the validity of the Model being passed to this method by way of any annotations specified on the model class.
            if (!ModelState.IsValid)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailed });

            //Get the user id of the person viewing the page
            var allThisWeeksReservations = ThisWeeksLaundrySignup();

            var usersReservationsThisWeek = allThisWeeksReservations.FindAll(reserved => reserved.UserId == WebSecurity.GetUserId(User.Identity.Name));
            var amountSignedUp = usersReservationsThisWeek.Count();

            //Check how many other times the person has signed up. This amount will be limited to less than 2 (for now)
            //IEnumerable<LaundrySignup> MySignups = db.LaundrySignups.Where(x => x.UserId == ThisUserId);
            //int AmountSignedUp = MySignups.Count();

            if (amountSignedUp >= 2) //MAKE CONST SOMEWHERE
            {
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailedTooMany });
            }

            //Add reservation to the database
            var reservation = new LaundrySignup
            {
                UserId = WebSecurity.GetUserId(User.Identity.Name),
                DateTimeShift = signup.Shift,
                DateTimeSignedUp = DateTime.Now
            };
            if (ModelState.IsValid)
            {
                uow.LaundrySignupRepository.Insert(reservation);
                uow.Save();
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveSuccess });
            }
            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.ReserveFailed });
        }
        [HttpPost]
        public ActionResult CancelReserveLaundry(LaundryReservationModel cancel)
        {
            if (uow.LaundrySignupRepository.Get(s => s.DateTimeShift == cancel.Shift) == null)
                return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });

            uow.LaundrySignupRepository.DeleteByShift(cancel.Shift);
            uow.Save();

            return RedirectToAction("LaundrySchedule", new { Message = LaundrySignupMessage.CancelReservationSuccess });
        }

        private String LaundrySummary()
        {
            var presentSignupSlot = DateTimeFloor(DateTime.Now, new TimeSpan(2, 0, 0));
            var nextSignupSlot = presentSignupSlot + new TimeSpan(2, 0, 0);
            var allThisWeeksReservations = ThisWeeksLaundrySignup().FindAll(reserved => reserved.Shift >= presentSignupSlot).OrderBy(x => x.Shift).ToList();

            if (allThisWeeksReservations.Count == 0)   // Scenario 3: No reservations this week -> special message
            {
                return "Laundry Room has not been reserved this week!";
            }
            //Scenario 1: Room is reserved now -> find next available time open
            //Scenario 2: Room not reserved now AND Room reserved next hour AND we are more than half way through the present shift -> pretent present shift is reserved and find next availabe time open
            //I HAVE NO IDEA IF SCENARIO 2 IS CORRECT!!!
            double hours;
            string hoursString;
            if (allThisWeeksReservations[0].Shift == presentSignupSlot || ((allThisWeeksReservations[0].Shift == nextSignupSlot) && (nextSignupSlot - DateTime.Now).TotalHours < 1))
            {
                var firstFreeTime = allThisWeeksReservations[0].Shift;
                for (var i = 0; i <= allThisWeeksReservations.Count; i++)
                {
                    if (i == allThisWeeksReservations.Count || allThisWeeksReservations[i].Shift != firstFreeTime)
                    {
                        //double hours = (allThisWeeksReservations[i - 1].Shift - DateTime.Now + new TimeSpan(2, 0, 0)).TotalHours;
                        hours = (firstFreeTime - DateTime.Now).TotalHours;
                        hoursString = hours.ToString("0.0");
                        return "Laundry Room is next available in " + hoursString + "hours.";
                    }
                    firstFreeTime += new TimeSpan(2, 0, 0);
                }
                //This line will never be called. The if statement above handles all possible inputs. This is just to eliminate an error.
                return "Laundry Room Information is not availale right now.";
            }

            hours = (allThisWeeksReservations[0].Shift - DateTime.Now).TotalHours;
            hoursString = hours.ToString("0.0");
            return "Laundry Room is available for the next " + hoursString + " hours.";
        }
        private List<LaundryReservationModel> ThisWeeksLaundrySignup()
        {
            // Beginning of today (12:00am of today).
            var today = DateTimeFloor(DateTime.Now, new TimeSpan(1, 0, 0, 0));
            var sevenDays = new TimeSpan(7, 0, 0, 0);
            // Add seven days to beginning of today (12:00am of a week from today).
            var endOfWeek = today + sevenDays;

            // All reservations within specified time frame.
            var signups = uow.LaundrySignupRepository
                .GetAll()
                .Where(l => l.DateTimeShift >= today && l.DateTimeShift < endOfWeek).ToList();
            var members = uow.MemberRepository.GetAll().ToList(); //all users

            var presentWeekSignup = new List<LaundryReservationModel>();
            foreach (var reservedTime in signups)
            {
                //Get user information for each of the reservations that take place this week
                var isReserved = members.Any(user => user.UserId == reservedTime.UserId);
                if (!isReserved) continue;

                var reservingMember = members.First(user => user.UserId == reservedTime.UserId);
                presentWeekSignup.Add(new LaundryReservationModel()
                {
                    UserId = reservedTime.UserId,
                    UserName = reservingMember.UserName,
                    Name = reservingMember.FirstName + " " + reservingMember.LastName[0] + ".",
                    Shift = reservedTime.DateTimeShift
                });
            }
            return presentWeekSignup;
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