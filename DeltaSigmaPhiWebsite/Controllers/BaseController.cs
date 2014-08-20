namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using System.Web.Security;

    public class BaseController : Controller
    {
        internal IWebSecurity WebSecurity { get; set; }
        internal IOAuthWebSecurity OAuthWebSecurity { get; set; }

        protected readonly IUnitOfWork uow = new UnitOfWork();
        public BaseController(IUnitOfWork uow, IWebSecurity webSecurity, IOAuthWebSecurity oAuthWebSecurity)
        {
            this.uow = uow;
            this.WebSecurity = webSecurity;
            this.OAuthWebSecurity = oAuthWebSecurity;
        }

        protected virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
        protected virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }
        protected virtual IEnumerable<Member> GetAllActiveMembers()
        {
            return uow.MemberRepository.SelectBy(m => m.MemberStatus.StatusName == "Active");
        }
        protected virtual IEnumerable<Member> GetAllPledgeMembers()
        {
            return uow.MemberRepository.SelectBy(m => m.MemberStatus.StatusName == "Pledge");
        }
        protected virtual IEnumerable<Member> GetAllActivePledgeNeophyteMembers()
        {
            return uow.MemberRepository.SelectBy(m => m.MemberStatus.StatusName == "Active" ||
                m.MemberStatus.StatusName == "Pledge" ||
                m.MemberStatus.StatusName == "Neophyte").ToList();
        }
        protected virtual IEnumerable<Member> GetAllAlumniMembers()
        {
            return uow.MemberRepository.SelectBy(m => m.MemberStatus.StatusName == "Alumnus");
        }
        protected virtual int? GetThisSemestersId()
        {
            var semesters = uow.SemesterRepository.SelectAll().ToList();
            if (semesters.Count <= 0) return null;

            try
            {
                var thisSemester = GetThisSemester();
                return thisSemester.SemesterId;
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected virtual IEnumerable<Semester> GetThisAndNextSemesterList()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            var thisAndComingSemesters = uow.SemesterRepository.SelectAll()
                .Where(s => s.DateEnd >= now)
                .OrderBy(s => s.DateStart)
                .Take(2)
                .ToList();

            return thisAndComingSemesters;
        }
        protected virtual IEnumerable<SelectListItem> GetThisAndNextSemesterSelectList()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            var thisAndComingSemesters = uow.SemesterRepository.SelectAll()
                .Where(s => s.DateEnd >= now)
                .OrderBy(s => s.DateStart)
                .ToList();

            var newList = new List<object>
            {
                new
                {
                    thisAndComingSemesters[0].SemesterId,
                    Name = thisAndComingSemesters[0].ToString()
                },
                new
                {
                    thisAndComingSemesters[1].SemesterId,
                    Name = thisAndComingSemesters[1].ToString()
                }
            };

            return new SelectList(newList, "SemesterId", "Name");
        }
        protected virtual Semester GetThisOrLastSemester()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            return uow.SemesterRepository.SelectAll()
                .Where(s => s.DateEnd <= now)
                .OrderBy(s => s.DateStart)
                .ToList()
                .Last();
        }
        protected virtual Semester GetThisSemester()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            return uow.SemesterRepository.SelectAll()
                    .Where(s => s.DateEnd >= now)
                    .OrderBy(s => s.DateStart)
                    .ToList()
                    .First();
        }
        protected virtual IEnumerable<SelectListItem> GetSemesterList()
        {
            var semesters = uow.SemesterRepository.SelectAll().OrderByDescending(s => s.DateEnd).ToList();
            var newList = new List<object>();

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name", GetThisSemester().SemesterId);
        }
        protected virtual IEnumerable<SelectListItem> GetSemesterListWithNone()
        {
            var semesters = uow.SemesterRepository.SelectAll().OrderByDescending(s => s.DateEnd).ToList();
            var newList = new List<object> { new { SemesterId = -1, Name = "--Graduating Semester (optional)--" } };

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name");
        }
        protected virtual IEnumerable<SelectListItem> GetUserIdListAsFullName()
        {
            var members = GetAllActivePledgeNeophyteMembers().OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName 
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual IEnumerable<SelectListItem> GetUserIdListAsFullNameWithNone()
        {
            return new SelectList(GetUserIdListAsFullNameWithNoneNonSelectList(), "UserId", "Name");
        }
        protected virtual IEnumerable<object> GetUserIdListAsFullNameWithNoneNonSelectList()
        {
            var members = GetAllActivePledgeNeophyteMembers().OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return newList;
        }
        protected virtual IEnumerable<SelectListItem> GetStatusList()
        {
            var statusList = uow.MemberStatusRepository.SelectAll();
            var newList = new List<object>();
            foreach (var status in statusList)
            {
                newList.Add(new
                {
                    status.StatusId,
                    status.StatusName
                });
            }
            return new SelectList(newList, "StatusId", "StatusName");
        }
        protected virtual IEnumerable<SelectListItem> GetStatusListWithNone()
        {
            var statusList = uow.MemberStatusRepository.SelectAll();
            var newList = new List<object> { new { StatusId = -1, StatusName = "--Status (optional)--" } };
            foreach (var status in statusList)
            {
                newList.Add(new
                {
                    status.StatusId,
                    status.StatusName
                });
            }
            return new SelectList(newList, "StatusId", "StatusName");
        }
        protected virtual IEnumerable<SelectListItem> GetTerms()
        {
            var terms = new List<string>
            {
                "Spring", "Fall"
            };
            return new SelectList(terms);
        }
        protected virtual IEnumerable<SelectListItem> GetPositionsList()
        {
            var positions = Roles.GetAllRoles();
            return new SelectList(positions);
        }
        protected virtual IEnumerable<SelectListItem> GetPledgeClassList()
        {
            var pledgeClasses = uow.PledgeClassRepository.SelectAll().OrderByDescending(s => s.Semester.DateEnd).ToList();
            var newList = new List<object>();

            foreach (var p in pledgeClasses)
            {
                newList.Add(new
                {
                    p.PledgeClassId,
                    p.PledgeClassName
                });
            }

            return new SelectList(newList, "PledgeClassId", "PledgeClassName");
        }
        protected virtual IEnumerable<SelectListItem> GetPledgeClassListWithNone()
        {
            var pledgeClasses = uow.PledgeClassRepository.SelectAll().OrderByDescending(s => s.Semester.DateEnd).ToList();
            var newList = new List<object> { new { PledgeClassId = -1, PledgeClassName = "--Pledge Class (optional)--" } };

            foreach (var p in pledgeClasses)
            {
                newList.Add(new
                {
                    p.PledgeClassId,
                    p.PledgeClassName
                });
            }

            return new SelectList(newList, "PledgeClassId", "PledgeClassName");
        }
        protected virtual IEnumerable<Leader> GetRecentAppointments()
        {
            var thisAndComingSemesters = uow.SemesterRepository.SelectAll()
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToList();

            var leaders = uow.LeaderRepository.SelectAll().ToList();

            var recentAppointsments = leaders
                .Where(l =>
                    l.SemesterId == thisAndComingSemesters[0].SemesterId ||
                    l.SemesterId == thisAndComingSemesters[1].SemesterId)
                .ToList();

            return recentAppointsments;
        }
        protected int GetRemainingServiceHoursForUser(int userId)
        {
            const int requiredHours = 15;
            // Beginning of today (12:00am of today)
            var today = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0));
            // Find current semester
            var currentSemester = uow.SemesterRepository.Single(s => s.DateStart <= today && s.DateEnd >= today);
            if (currentSemester == null)
                return 15;

            var startOfSemester = currentSemester.DateStart;
            var endOfSemester = currentSemester.DateEnd;

            var totalHours = 0.0;
            try
            {
                var serviceHours = uow.ServiceHourRepository.SelectAll()
                    .Where(h => h.Member.UserId == userId && h.Event.DateTimeOccurred >= startOfSemester &&
                            h.Event.DateTimeOccurred <= endOfSemester);
                if (serviceHours.Any())
                    totalHours = serviceHours.Select(h => h.DurationHours).Sum();
            }
            catch (Exception)
            {

            }

            var soberDriverCheck = uow.SoberSignupsRepository.SelectAll()
                .Any(s =>
                    s.UserId == userId &&
                    s.Type == SoberSignupType.Driver &&
                    s.DateOfShift >= startOfSemester &&
                    s.DateOfShift <= endOfSemester);
            if (soberDriverCheck)
            {
                totalHours += 5;
            }

            var remainingHours = requiredHours - (int)totalHours;
            return remainingHours < 0 ? 0 : remainingHours;
        }
        protected IEnumerable<SelectListItem> GetAllEventIdsAsEventName()
        {
            var today = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0)); //beginning of today (12:00am of today)
            var yearAgoToday = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0)); //beginning of today, one year ago (12:00am of today)
            yearAgoToday -= new TimeSpan(365, 0, 0, 0);
            var events = uow.EventRepository.SelectBy(e => (e.DateTimeOccurred <= today && e.DateTimeOccurred >= yearAgoToday)); //only retrieves events ranging one year back
            return new SelectList(events, "EventId", "EventName");
        }
        protected IEnumerable<ServiceHour> GetAllCompletedEventsForUser(int userId)
        {
            var thisSemester = GetThisSemester();
            if (thisSemester == null)
            {
                return uow.ServiceHourRepository.SelectBy(e => e.UserId == userId);
            }

            return uow.ServiceHourRepository.SelectBy(e => 
                e.UserId == userId &&
                e.Event.DateTimeOccurred >= thisSemester.DateStart &&
                e.Event.DateTimeOccurred <= thisSemester.DateEnd);
        }
        protected IEnumerable<SoberSignup> GetSoberSignupsForUser(int userId, Semester semester)
        {
            return uow.SoberSignupsRepository.SelectAll().Where(s =>
                    s.UserId == userId &&
                    s.DateOfShift <= semester.DateEnd &&
                    s.DateOfShift >= semester.DateStart);
        }
        protected IEnumerable<SelectListItem> GetAllApproverIds(int userId)
        {
            var members = uow.MemberRepository.SelectBy(a => a.UserId != userId).OrderBy(o => o.LastName);
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
        protected int GetRemainingStudyHoursForUser(int userId)
        {
            var startOfThisWeek = GetStartOfCurrentWeek();
            var member = uow.MemberRepository.SingleById(userId);
            var totalHours = 0.0;

            try
            {
                totalHours = (from hour in uow.StudyHourRepository.SelectAll()
                              where hour.SubmittedBy == userId &&
                                    hour.ApproverId != null &&
                                    hour.DateTimeStudied >= startOfThisWeek
                              select hour.DurationHours).ToList().Sum();
            }
            catch (Exception)
            {

            }

            return (member.RequiredStudyHours - (int)totalHours);
        }
        protected IEnumerable<StudyHour> GetStudyHoursForUser(int userId)
        {
            var member = uow.MemberRepository.SingleById(userId);

            return member.SubmittedStudyHours.Where(s => s.DateTimeStudied >= GetStartOfCurrentWeek());
        }
        protected DateTime GetStartOfCurrentWeek()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return DateTime.Today.AddDays(-5);
                case DayOfWeek.Monday:
                    return DateTime.Today.AddDays(-6);
                case DayOfWeek.Tuesday:
                    return DateTime.Today;
                case DayOfWeek.Wednesday:
                    return DateTime.Today.AddDays(-1);
                case DayOfWeek.Thursday:
                    return DateTime.Today.AddDays(-2);
                case DayOfWeek.Friday:
                    return DateTime.Today.AddDays(-3);
                default: // Saturday
                    return DateTime.Today.AddDays(-4);
            }
        }

        protected DateTime DateTimeFloor(DateTime date, TimeSpan span)
        {
            //Rounds down based on the TimeSpan
            //Ex. date = DateTime.UtcNow, span = TimeSpan of one day
            //Results in 12:00am of today
            var ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}