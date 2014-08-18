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

        protected virtual int? GetThisSemestersId()
        {
            var semesters = uow.SemesterRepository.GetAll().ToList();
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
            var thisAndComingSemesters = uow.SemesterRepository.GetAll()
                .Where(s => s.DateEnd >= DateTime.Now)
                .OrderBy(s => s.DateStart)
                .Take(2)
                .ToList();

            return thisAndComingSemesters;
        }
        protected virtual IEnumerable<SelectListItem> GetThisAndNextSemesterSelectList()
        {
            var thisAndComingSemesters = uow.SemesterRepository.GetAll()
                .Where(s => s.DateEnd >= DateTime.Now)
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
            return uow.SemesterRepository.GetAll()
                .Where(s => s.DateEnd <= DateTime.Now)
                .OrderBy(s => s.DateStart)
                .ToList()
                .Last();
        }
        protected virtual Semester GetThisSemester()
        {
            return uow.SemesterRepository.GetAll()
                    .Where(s => s.DateEnd >= DateTime.Now)
                    .OrderBy(s => s.DateStart)
                    .ToList()
                    .First();
        }

        protected virtual IEnumerable<SelectListItem> GetSemesterList()
        {
            var semesters = uow.SemesterRepository.GetAll().OrderByDescending(s => s.DateEnd).ToList();
            var newList = new List<object>();

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
        protected virtual IEnumerable<SelectListItem> GetSemesterListWithNone()
        {
            var semesters = uow.SemesterRepository.GetAll().OrderByDescending(s => s.DateEnd).ToList();
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
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
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
        protected virtual IEnumerable<SelectListItem> GetUserIdListAsFullNameWithNone()
        {
            return new SelectList(GetUserIdListAsFullNameWithNoneNonSelectList(), "UserId", "Name");
        }
        protected virtual IEnumerable<object> GetUserIdListAsFullNameWithNoneNonSelectList()
        {
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return newList;
        }
        protected virtual IEnumerable<SelectListItem> GetStatusList()
        {
            var statusList = uow.MemberStatusRepository.GetAll();
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
            var statusList = uow.MemberStatusRepository.GetAll();
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
            var pledgeClasses = uow.PledgeClassRepository.GetAll().OrderByDescending(s => s.Semester.DateEnd).ToList();
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
            var pledgeClasses = uow.PledgeClassRepository.GetAll().OrderByDescending(s => s.Semester.DateEnd).ToList();
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
            var thisAndComingSemesters = uow.SemesterRepository.GetAll()
                .Where(s => s.DateEnd >= DateTime.Now)
                .OrderBy(s => s.DateStart)
                .ToList();

            var leaders = uow.LeaderRepository.GetAll().ToList();

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
            var today = DateTimeFloor(DateTime.Now, new TimeSpan(1, 0, 0, 0));
            // Find current semester
            var currentSemester = uow.SemesterRepository.Get(s => s.DateStart <= today && s.DateEnd >= today);
            if (currentSemester == null)
                return 15;

            var startOfSemester = currentSemester.DateStart;
            var endOfSemester = currentSemester.DateEnd;

            var totalHours = 0.0;
            try
            {
                var serviceHours = uow.ServiceHourRepository.GetAll()
                    .Where(h => h.Member.UserId == userId && h.Event.DateTimeOccurred >= startOfSemester &&
                            h.Event.DateTimeOccurred <= endOfSemester);
                if (serviceHours.Any())
                    totalHours = serviceHours.Select(h => h.DurationHours).Sum();
            }
            catch (Exception)
            {

            }

            var soberDriverCheck = uow.SoberSignupsRepository.GetAll()
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
            var today = DateTimeFloor(DateTime.Now, new TimeSpan(1, 0, 0, 0)); //beginning of today (12:00am of today)
            var yearAgoToday = DateTimeFloor(DateTime.Now, new TimeSpan(1, 0, 0, 0)); //beginning of today, one year ago (12:00am of today)
            yearAgoToday -= new TimeSpan(365, 0, 0, 0);
            var events = uow.EventRepository.FindBy(e => (e.DateTimeOccurred <= today && e.DateTimeOccurred >= yearAgoToday)); //only retrieves events ranging one year back
            return new SelectList(events, "EventId", "EventName");
        }
        protected IEnumerable<ServiceHour> GetAllCompletedEventsForUser(int userId)
        {
            var thisSemester = GetThisSemester();
            if (thisSemester == null)
            {
                return uow.ServiceHourRepository.FindBy(e => e.UserId == userId);
            }

            return uow.ServiceHourRepository.FindBy(e => 
                e.UserId == userId &&
                e.Event.DateTimeOccurred >= thisSemester.DateStart &&
                e.Event.DateTimeOccurred <= thisSemester.DateEnd);
        }

        protected IEnumerable<SoberSignup> GetSoberSignupsForUser(int userId, Semester semester)
        {
            return uow.SoberSignupsRepository.GetAll().Where(s =>
                    s.UserId == userId &&
                    s.DateOfShift <= semester.DateEnd &&
                    s.DateOfShift >= semester.DateStart);
        }

        protected DateTime DateTimeFloor(DateTime date, TimeSpan span)
        {
            //Rounds down based on the TimeSpan
            //Ex. date = DateTime.Now, span = TimeSpan of one day
            //Results in 12:00am of today
            var ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }

        protected virtual IEnumerable<ExternalLogin> GetExternalLogins(string userName)
        {
            var accounts = OAuthWebSecurity.GetAccountsFromUserName(userName);
            var externalLogins = (
                from account in accounts
                let clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider)
                select new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId
                }
            ).ToList();
            return externalLogins;
        }
        public virtual string GetPictureUrl(string userName)
        {
            var logins = GetExternalLogins(userName).ToList();
            if (logins.Count <= 0) return "";
            var facebookId = (from login in logins where login.ProviderDisplayName == "Facebook" select login.ProviderUserId).First();

            WebResponse response = null;
            var pictureUrl = string.Empty;
            try
            {
                var request = WebRequest.Create(string.Format("https://graph.facebook.com/{0}/picture", facebookId));
                response = request.GetResponse();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                //? handle
            }
            finally
            {
                if (response != null) response.Close();
            }
            return pictureUrl;
        }
        public virtual string GetBigPictureUrl(string userName)
        {
            var logins = GetExternalLogins(userName).ToList();
            if (logins.Count <= 0) return "";
            var facebookId = (from login in logins where login.ProviderDisplayName == "Facebook" select login.ProviderUserId).First();

            WebResponse response = null;
            var pictureUrl = string.Empty;
            try
            {
                var request = WebRequest.Create(string.Format("https://graph.facebook.com/{0}/picture?type=large", facebookId));
                response = request.GetResponse();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                //? handle
            }
            finally
            {
                if (response != null) response.Close();
            }
            return pictureUrl;
        }

        protected override void Dispose(bool disposing)
        {
            uow.Dispose();
            base.Dispose(disposing);
        }
    }
}