namespace Dsp.Web.Controllers
{
    using Areas.Scholarships.Models;
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    public class BaseController : Controller
    {
        protected const string SuccessMessageKey = "SuccessMessage";
        protected const string FailureMessageKey = "FailureMessage";

        protected ApplicationSignInManager _signInManager;
        protected ApplicationUserManager _userManager;
        protected ApplicationRoleManager _roleManager;
        protected readonly SphinxDbContext _db = new SphinxDbContext();

        public BaseController()
        {
        }
        public BaseController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        protected virtual async Task<List<Member>> GetAllActiveMembersAsync()
        {
            return await _db.Users.Where(m => m.MemberStatus.StatusName == "Active").ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllPledgeMembersAsync()
        {
            return await _db.Users.Where(m => m.MemberStatus.StatusName == "Pledge").ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllActivePledgeNeophyteMembersAsync()
        {
            return await _db.Users
                .Where(m => m.MemberStatus.StatusName == "Active" ||
                            m.MemberStatus.StatusName == "Pledge" ||
                            m.MemberStatus.StatusName == "Neophyte")
                .ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllAlumniMembersAsync()
        {
            return await _db.Users.Where(m => m.MemberStatus.StatusName == "Alumnus").ToListAsync();
        }
        protected virtual async Task<List<Member>> GetRosterForSemester(Semester semester)
        {
            return await _db.Users
                .Where(d =>
                    d.LastName != "Hirtz" &&
                    (d.MemberStatus.StatusName == "Alumnus" ||
                    d.MemberStatus.StatusName == "Active" ||
                    d.MemberStatus.StatusName == "Pledge" ||
                    d.MemberStatus.StatusName == "Neophyte") &&
                    d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                    d.GraduationSemester.DateEnd > semester.DateStart)
                .ToListAsync();
        }
        protected virtual async Task<List<Member>> GetRosterForSemesterPlusReleased(Semester semester)
        {
            return await _db.Users
                .Where(d =>
                    d.LastName != "Hirtz" &&
                    (d.MemberStatus.StatusName == "Alumnus" ||
                    d.MemberStatus.StatusName == "Active" ||
                    d.MemberStatus.StatusName == "Pledge" ||
                    d.MemberStatus.StatusName == "Neophyte" ||
                    d.MemberStatus.StatusName == "Released") &&
                    d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                    d.GraduationSemester.DateEnd > semester.DateStart)
                .ToListAsync();
        }
        protected virtual async Task<IEnumerable<Semester>> GetThisAndNextSemesterListAsync()
        {
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .Take(2)
                .ToListAsync();

            return thisAndComingSemesters;
        }
        protected virtual async Task<SelectList> GetThisAndNextSemesterSelectListAsync()
        {
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync();

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
        protected virtual async Task<Semester> GetThisOrLastSemesterAsync()
        {
            return (await _db.Semesters
                .Where(s => s.DateEnd <= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .Last();
        }
        protected virtual async Task<Semester> GetThisSemesterAsync()
        {
            return (await _db.Semesters
                    .Where(s => s.DateEnd >= DateTime.UtcNow)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .First();
        }
        protected virtual async Task<Semester> GetLastSemesterAsync()
        {
            return (await _db.Semesters
                    .Where(s => s.DateEnd < DateTime.UtcNow)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .Last();
        }
        protected virtual async Task<int?> GetThisSemestersIdAsync()
        {
            var semesters = await _db.Semesters.ToListAsync();
            if (semesters.Count <= 0) return null;

            try
            {
                var thisSemester = await GetThisSemesterAsync();
                return thisSemester.SemesterId;
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected virtual async Task<Semester> GetSemestersForUtcDateAsync(DateTime date)
        {
            return (await _db.Semesters
                    .Where(s => s.DateEnd >= date)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .First();
        }
        protected virtual async Task<IEnumerable<object>> GetUserIdListAsFullNameWithNoneNonSelectListAsync()
        {
            var members = (await GetAllActivePledgeNeophyteMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return newList;
        }
        protected virtual async Task<SelectList> GetAllUserIdsSelectListAsFullNameWithNoneAsync()
        {
            var members = (await _db.Users.ToListAsync()).OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<IEnumerable<object>> GetAlumniIdListAsFullNameWithNoneNonSelectListAsync()
        {
            var members = (await GetAllAlumniMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return newList;
        }
        protected virtual async Task<SelectList> GetAllSemesterListAsync()
        {
            var semesters = await _db.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
            var newList = new List<object>();

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name", (await GetThisSemesterAsync()).SemesterId);
        }
        protected virtual async Task<SelectList> GetSemesterListAsync()
        {
            var currentSemester = await GetThisSemesterAsync();
            var semesters = await _db.Semesters
                .Where(s => s.DateEnd <= currentSemester.DateEnd)
                .OrderByDescending(s => s.DateEnd).ToListAsync();
            var newList = new List<object>();

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name", (await GetThisSemesterAsync()).SemesterId);
        }
        protected virtual async Task<SelectList> GetSemesterListWithNoneAsync()
        {
            var semesters = await _db.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
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
        protected virtual SelectList GetCustomSemesterListAsync(IEnumerable<Semester> list)
        {
            var newList = new List<object>();

            foreach (var s in list.OrderByDescending(s => s.DateEnd))
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name");
        }
        protected virtual async Task<SelectList> GetUserIdListAsFullNameAsync()
        {
            var members = (await GetAllActivePledgeNeophyteMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetPledgeUserIdListAsFullNameAsync()
        {
            var members = (await GetAllPledgeMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetGraduatingActiveUserIdListAsFullNameAsync()
        {
            var thisAndPreviousSemesters = await _db.Semesters
                .Where(s => s.DateEnd <= DateTime.UtcNow)
                .ToListAsync();
            var semesterIds = thisAndPreviousSemesters.Select(s => (int?)s.SemesterId);
            var members = UserManager.Users
                .Where(m =>
                    (m.ExpectedGraduationId == null || semesterIds.Contains(m.ExpectedGraduationId))
                    && m.MemberStatus.StatusName == "Active")
                .OrderBy(m => m.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetUserIdListAsFullNameWithNoneAsync()
        {
            return new SelectList(await GetUserIdListAsFullNameWithNoneNonSelectListAsync(), "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetAllUserIdsListAsFullNameWithNoneAsync()
        {
            return new SelectList(await GetUserIdListAsFullNameWithNoneNonSelectListAsync(), "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetStatusListAsync()
        {
            var statusList = await _db.MemberStatuses.ToListAsync();
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
        protected virtual async Task<SelectList> GetStatusListWithNoneAsync()
        {
            var statusList = await _db.MemberStatuses.ToListAsync();
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
        protected virtual async Task<SelectList> GetPledgeClassListAsync()
        {
            var pledgeClasses = await _db.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
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
        protected virtual async Task<SelectList> GetPledgeClassListWithNoneAsync()
        {
            var pledgeClasses = await _db.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
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
        protected virtual async Task<IEnumerable<Leader>> GetRecentAppointmentsAsync()
        {
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync();



            var recentAppointsments = new List<Leader>();

            return recentAppointsments;
        }
        protected virtual async Task<double> GetRemainingServiceHoursForUserAsync(int userId)
        {
            const double requiredHours = 10;

            var lastSemester = await GetLastSemesterAsync();
            var currentSemester = await GetThisSemesterAsync();

            var totalHours = 0.0;
            try
            {
                var serviceHours = await _db.ServiceHours
                    .Where(h => h.Member.Id == userId &&
                                h.Event.DateTimeOccurred > lastSemester.DateEnd &&
                                h.Event.DateTimeOccurred <= currentSemester.DateEnd)
                    .ToListAsync();
                if (serviceHours.Any())
                    totalHours = serviceHours.Select(h => h.DurationHours).Sum();
            }
            catch (Exception)
            {
                return requiredHours - totalHours < 0 ? 0 : requiredHours - totalHours;
            }

            var remainingHours = requiredHours - totalHours;
            return remainingHours < 0 ? 0 : remainingHours;
        }
        protected virtual async Task<SelectList> GetAllEventIdsAsEventNameAsync()
        {
            var lastSemester = await GetLastSemesterAsync();
            var thisSemester = await GetThisSemesterAsync();

            var events = await _db.ServiceEvents
                .Where(e =>
                    e.DateTimeOccurred > lastSemester.DateEnd &&
                    e.DateTimeOccurred <= thisSemester.DateEnd)
                .OrderByDescending(o => o.DateTimeOccurred)
                .ToListAsync();

            var newList = new List<object>();
            foreach (var e in events)
            {
                newList.Add(new
                {
                    e.EventId,
                    EventName = ConvertUtcToCst(e.DateTimeOccurred) + ": " + e.EventName + " (Lasted " + e.DurationHours + "hrs)"
                });
            }

            return new SelectList(newList, "EventId", "EventName");
        }
        protected virtual async Task<SelectList> GetAllEventIdsAsEventNameAsync(int semesterId)
        {
            var thisSemester = await _db.Semesters.FindAsync(semesterId);
            var lastSemester = await _db.Semesters
                .Where(s => s.DateStart < thisSemester.DateStart)
                .OrderByDescending(s => s.DateStart)
                .FirstAsync();

            var events = await _db.ServiceEvents
                .Where(e =>
                    e.DateTimeOccurred > lastSemester.DateEnd &&
                    e.DateTimeOccurred <= thisSemester.DateEnd)
                .OrderByDescending(o => o.DateTimeOccurred)
                .ToListAsync();

            var newList = new List<object>();
            foreach (var e in events)
            {
                newList.Add(new
                {
                    e.EventId,
                    EventName = ConvertUtcToCst(e.DateTimeOccurred) + ": " + e.EventName + " (Lasted " + e.DurationHours + "hrs)"
                });
            }

            return new SelectList(newList, "EventId", "EventName");
        }
        protected virtual async Task<IEnumerable<ServiceHour>> GetAllCompletedEventsForUserAsync(int userId)
        {
            var thisSemester = await GetThisSemesterAsync();
            var lastSemester = await GetLastSemesterAsync();

            return await _db.ServiceHours
                .Where(e => e.UserId == userId &&
                            e.Event.DateTimeOccurred > lastSemester.DateEnd &&
                            e.Event.DateTimeOccurred <= thisSemester.DateEnd)
                .ToListAsync();
        }
        protected virtual async Task<IEnumerable<SoberSignup>> GetSoberSignupsForUserAsync(int userId, Semester semester)
        {
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < semester.DateStart)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .Last();

            return await _db.SoberSignups
                .Where(s => s.UserId == userId &&
                            s.DateOfShift > previousSemester.DateEnd &&
                            s.DateOfShift <= semester.DateEnd)
                .ToListAsync();
        }
        protected virtual async Task<List<SoberSignup>> GetSoberSignupsNextSevenDaysAsync(DateTime date)
        {
            var startOfTodayCst = ConvertUtcToCst(date).Date;
            var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            var sevenDaysAheadOfToday = startOfTodayUtc.AddDays(7);
            var thisSemester = await GetThisSemesterAsync();
            var soberSignups = await _db.SoberSignups
                .Where(s => s.DateOfShift >= startOfTodayUtc &&
                            s.DateOfShift <= thisSemester.DateEnd)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.SoberTypeId)
                .ToListAsync();
            var data = soberSignups
                .Where(s => s.DateOfShift >= startOfTodayUtc &&
                            s.DateOfShift <= sevenDaysAheadOfToday)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.SoberTypeId)
                .ToList();
            return data;
        }
        protected virtual async Task<List<SoberSignup>> GetThisWeeksSoberSignupsAsync(DateTime now)
        {
            var nowUtc = now;
            var nowCst = this.ConvertUtcToCst(nowUtc);

            // This makes the week restart at 4am on Sundays so that users can check the list
            // up until 4am in the morning before it rolls over.
            if (nowCst.Hour < 4 && nowCst.DayOfWeek == DayOfWeek.Sunday)
            {
                nowCst = nowCst.AddDays(-1);
            }

            var startOfTodayCst = nowCst.Date;
            var startOfTodayUtc = this.ConvertCstToUtc(startOfTodayCst);
            var weekOfYear = DateTimeExtensions.GetWeekOfYear(startOfTodayUtc);
            var startOfThisWeek = DateTimeExtensions.FirstDateOfWeek(startOfTodayUtc.Year, weekOfYear, CultureInfo.GetCultureInfo("en-US"));
            var startOfNextWeek = startOfThisWeek.AddDays(7);

            return await _db.SoberSignups
                .Where(s =>
                    s.DateOfShift >= startOfThisWeek &&
                    s.DateOfShift < startOfNextWeek)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.SoberTypeId)
                .ToListAsync();
        }
        public virtual DateTime GetStartOfCurrentWeek()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow).Date;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return now;
                case DayOfWeek.Monday:
                    return now.AddDays(-1);
                case DayOfWeek.Tuesday:
                    return now.AddDays(-2);
                case DayOfWeek.Wednesday:
                    return now.AddDays(-3);
                case DayOfWeek.Thursday:
                    return now.AddDays(-4);
                case DayOfWeek.Friday:
                    return now.AddDays(-5);
                default: // Saturday
                    return now.AddDays(-6);
            }
        }
        protected virtual DateTime GetStartOfCurrentStudyWeek()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow).Date;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return now.AddDays(-5);
                case DayOfWeek.Monday:
                    return now.AddDays(-6);
                case DayOfWeek.Tuesday:
                    return now;
                case DayOfWeek.Wednesday:
                    return now.AddDays(-1);
                case DayOfWeek.Thursday:
                    return now.AddDays(-2);
                case DayOfWeek.Friday:
                    return now.AddDays(-3);
                default: // Saturday
                    return now.AddDays(-4);
            }
        }
        public virtual DateTime DateTimeFloor(DateTime date, TimeSpan span)
        {
            //Rounds down based on the TimeSpan
            //Ex. date = DateTime.UtcNow, span = TimeSpan of one day
            //Results in 12:00am of today
            var ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }
        public virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
        public virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }
        protected virtual SelectList GetShirtSizesSelectList()
        {
            var newList = new List<string> { "S", "M", "L", "XL", "2XL" };
            var list = new SelectList(newList.Select(x => new { Value = x, Text = x }), "Value", "Text");
            return list;
        }
        protected virtual IEnumerable<object> GetGrades()
        {
            var gradeList = new List<string> { "", "A", "B", "C", "D", "F", "I", "S", "U" };
            var list = new List<object>();
            foreach (var g in gradeList)
            {
                list.Add(new { Value = g, Text = g });
            }
            return list;
        }
        protected virtual SelectList GetTerms()
        {
            var terms = new List<string>
            {
                "Spring", "Fall"
            };
            return new SelectList(terms);
        }
        protected virtual SelectList GetPositionsList()
        {
            var positions = Roles.GetAllRoles();
            return new SelectList(positions);
        }
        protected virtual async Task<SelectList> GetScholarshipTypesSelectListAsync()
        {
            var types = await _db.ScholarshipTypes.ToListAsync();
            var newList = new List<object>();

            foreach (var t in types)
            {
                newList.Add(new
                {
                    t.ScholarshipTypeId,
                    t.Name
                });
            }

            return new SelectList(newList, "ScholarshipTypeId", "Name");
        }
        protected virtual async Task<List<QuestionSelectionModel>> GetScholarshipQuestionsAsync()
        {
            var questions = await _db.ScholarshipQuestions.ToListAsync();
            var list = new List<QuestionSelectionModel>();
            foreach (var q in questions)
            {
                var appQuestion = new ScholarshipAppQuestion();
                appQuestion.ScholarshipQuestionId = q.ScholarshipQuestionId;
                appQuestion.FormOrder = 0;
                appQuestion.Question = q;

                var selection = new QuestionSelectionModel();
                selection.Question = appQuestion;
                list.Add(selection);
            }

            return list;
        }
        protected virtual async Task<SelectList> GetMealItemsSelectListAsync()
        {
            var list = await _db.MealItems.ToListAsync();
            var newList = new List<object>();
            foreach (var m in list)
            {
                newList.Add(new
                {
                    m.Id,
                    Text = m.Name
                });
            }
            return new SelectList(newList, "Id", "Text");
        }
        protected virtual IEnumerable<WorkOrder> GetFilteredWorkOrderList(
            IList<WorkOrder> workOrders,
            string s, string sort, int page, bool open, bool closed, int pageSize = 10)
        {
            var filterResults = new List<WorkOrder>();
            // Filter out results based on open, closed, sort, and/or search string.
            if (open)
            {
                var openResults = workOrders.Where(w => w.GetCurrentStatus() != "Closed").ToList();
                filterResults.AddRange(openResults);
            }
            if (closed)
            {
                var closedResults = workOrders.Where(w => w.GetCurrentStatus() == "Closed").ToList();
                filterResults.AddRange(closedResults);
            }
            switch (sort)
            {
                case "newest":
                    filterResults = filterResults.OrderByDescending(o => o.GetDateTimeCreated()).ToList();
                    break;
                case "oldest":
                    filterResults = filterResults.OrderBy(o => o.GetDateTimeCreated()).ToList();
                    break;
                case "most-commented":
                    filterResults = filterResults.OrderByDescending(o => o.Comments.Count).ToList();
                    break;
                case "least-commented":
                    filterResults = filterResults.OrderBy(o => o.Comments.Count).ToList();
                    break;
                case "recently-updated":
                    filterResults = filterResults.OrderByDescending(o => o.GetMostRecentActivityDateTime()).ToList();
                    break;
                case "least-recently-updated":
                    filterResults = filterResults.OrderBy(o => o.GetMostRecentActivityDateTime()).ToList();
                    break;
                default:
                    sort = "newest";
                    filterResults = filterResults.OrderByDescending(o => o.GetDateTimeCreated()).ToList();
                    break;
            }
            if (!string.IsNullOrEmpty(s))
            {
                s = s.ToLower();
                filterResults = filterResults
                    .Where(w =>
                        w.WorkOrderId.ToString() == s ||
                        w.Title.ToLower().Contains(s) ||
                        w.Member.FirstName.ToLower().Contains(s) ||
                        w.Member.LastName.ToLower().Contains(s) ||
                        w.GetCurrentPriority().ToLower().Contains(s) ||
                        w.GetCurrentStatus().ToLower().Contains(s))
                    .ToList();
            }
            ViewBag.OpenResultCount = filterResults.Count(w => w.GetCurrentStatus() != "Closed");
            ViewBag.ClosedResultCount = filterResults.Count(w => w.GetCurrentStatus() == "Closed");

            // Set search values so they carry over to the view.
            ViewBag.CurrentFilter = s;
            ViewBag.Open = open;
            ViewBag.Closed = closed;
            ViewBag.Sort = sort;

            if (page < 1) page = 1;
            ViewBag.Count = filterResults.Count;
            ViewBag.PageSize = pageSize;
            ViewBag.Pages = filterResults.Count / pageSize;
            ViewBag.Page = page;
            if (filterResults.Count % pageSize != 0) ViewBag.Pages += 1;
            if (page > ViewBag.Pages) ViewBag.Page = ViewBag.Pages;

            return filterResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        protected virtual IEnumerable<IncidentReport> GetFilteredIncidentsList(
            IList<IncidentReport> incidents,
            string s, string sort, int page, bool unresolved, bool resolved, int pageSize = 10)
        {
            var filterResults = new List<IncidentReport>();
            // Filter out results based on open, closed, sort, and/or search string.
            if (unresolved && resolved)
            {
                filterResults = incidents.ToList();
            }
            else if (unresolved)
            {
                var unresolvedResults = incidents.Where(i => string.IsNullOrEmpty(i.OfficialReport)).ToList();
                filterResults.AddRange(unresolvedResults);
            }
            else if (resolved)
            {
                var resolvedResults = incidents.Where(i => !string.IsNullOrEmpty(i.OfficialReport)).ToList();
                filterResults.AddRange(resolvedResults);
            }
            switch (sort)
            {
                case "oldest":
                    filterResults = filterResults.OrderBy(o => o.DateTimeSubmitted).ToList();
                    break;
                default:
                    sort = "newest";
                    filterResults = filterResults.OrderByDescending(o => o.DateTimeSubmitted).ToList();
                    break;
            }
            if (!string.IsNullOrEmpty(s))
            {
                s = s.ToLower();
                // Privileged search
                if (User.IsInRole("Administrator") || User.IsInRole("Sergeant-at-Arms") ||
                    User.IsInRole("Sergeant-at-Arms"))
                {
                    filterResults = filterResults
                    .Where(i =>
                        i.PolicyBroken.ToLower().Contains(s) ||
                        (!string.IsNullOrEmpty(i.OfficialReport) && i.OfficialReport.ToLower().Contains(s)) ||
                        i.Description.ToLower().Contains(s))
                    .ToList();
                }
                // Normal search
                else
                {
                    filterResults = filterResults
                    .Where(i =>
                        i.PolicyBroken.ToLower().Contains(s) ||
                        i.OfficialReport.ToLower().Contains(s))
                    .ToList();
                }

            }
            ViewBag.UnresolvedResultCount = filterResults.Count(i => string.IsNullOrEmpty(i.OfficialReport));
            ViewBag.ResolvedResultCount = filterResults.Count(i => !string.IsNullOrEmpty(i.OfficialReport));

            // Set search values so they carry over to the view.
            ViewBag.CurrentFilter = s;
            ViewBag.Unresolved = unresolved;
            ViewBag.Resolved = resolved;
            ViewBag.Sort = sort;

            if (page < 1) page = 1;
            ViewBag.Count = filterResults.Count;
            ViewBag.PageSize = pageSize;
            ViewBag.Pages = filterResults.Count / pageSize;
            ViewBag.Page = page;
            if (filterResults.Count % pageSize != 0) ViewBag.Pages += 1;
            if (page > ViewBag.Pages) ViewBag.Page = ViewBag.Pages;

            return filterResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        protected virtual async Task<IEnumerable<Class>> GetFilteredClassList(
            IList<Class> classes,
            string s, string sort, int page, string select, int pageSize = 20)
        {
            var filterResults = classes;
            var thisSemester = await GetThisSemesterAsync();
            var userId = User.Identity.GetUserId<int>();
            switch (select)
            {
                case "me-all":
                    filterResults = classes.Where(c =>
                        c.ClassesTaken.Any(t => t.UserId == userId)).ToList();
                    break;
                case "me-now":
                    filterResults = classes.Where(c =>
                        c.ClassesTaken.Any(t =>
                            t.UserId == userId &&
                            t.SemesterId == thisSemester.SemesterId &&
                            !t.IsSummerClass)).ToList();
                    break;
                case "being-taken":
                    filterResults = classes.Where(c =>
                        c.ClassesTaken.Any(t => t.SemesterId == thisSemester.SemesterId)).ToList();
                    break;
                case "none-taking":
                    filterResults = classes.Where(c =>
                        c.ClassesTaken.All(t => t.SemesterId != thisSemester.SemesterId)).ToList();
                    break;
                case "has-file":
                    filterResults = classes.Where(c => c.ClassFiles.Any()).ToList();
                    break;
                case "no-file":
                    filterResults = classes.Where(c => !c.ClassFiles.Any()).ToList();
                    break;
            }

            switch (sort)
            {
                case "number-desc":
                    filterResults = filterResults.OrderByDescending(o => o.CourseShorthand).ToList();
                    break;
                case "name-asc":
                    filterResults = filterResults.OrderBy(o => o.CourseName).ToList();
                    break;
                case "name-desc":
                    filterResults = filterResults.OrderByDescending(o => o.CourseName).ToList();
                    break;
                case "files-asc":
                    filterResults = filterResults.OrderBy(o => o.ClassFiles.Count).ThenBy(o => o.CourseShorthand).ToList();
                    break;
                case "files-desc":
                    filterResults = filterResults.OrderByDescending(o => o.ClassFiles.Count).ThenBy(o => o.CourseShorthand).ToList();
                    break;
                case "taken-asc":
                    filterResults = filterResults.OrderBy(o => o.ClassesTaken.Count).ThenBy(o => o.CourseShorthand).ToList();
                    break;
                case "taken-desc":
                    filterResults = filterResults.OrderByDescending(o => o.ClassesTaken.Count).ThenBy(o => o.CourseShorthand).ToList();
                    break;
                case "taking-asc":
                    filterResults = filterResults.OrderBy(o =>
                            o.ClassesTaken.Select(t => t.SemesterId == thisSemester.SemesterId).ToList().Count)
                        .ThenBy(o => o.CourseShorthand).ToList();
                    break;
                case "taking-desc":
                    filterResults = filterResults.OrderByDescending(o =>
                            o.ClassesTaken.Select(t => t.SemesterId == thisSemester.SemesterId).ToList().Count)
                        .ThenBy(o => o.CourseShorthand).ToList();
                    break;
                default:
                    sort = "number-asc";
                    filterResults = filterResults.OrderBy(o => o.CourseShorthand).ToList();
                    break;
            }
            if (!string.IsNullOrEmpty(s))
            {
                s = s.ToLower();
                filterResults = filterResults
                    .Where(i =>
                        i.CourseShorthand.ToLower().Contains(s) ||
                        i.CourseName.ToLower().Contains(s) ||
                        i.Department.Name.ToLower().Contains(s) ||
                        i.ClassesTaken.Any(t => t.Member.ToString().ToLower().Contains(s)))
                    .ToList();
            }

            // Set search values so they carry over to the view.
            ViewBag.CurrentFilter = s;
            ViewBag.Select = select;
            ViewBag.Sort = sort;

            if (page < 1) page = 1;
            ViewBag.Count = filterResults.Count();
            ViewBag.PageSize = pageSize;
            ViewBag.Pages = filterResults.Count / pageSize;
            ViewBag.Page = page;
            if (filterResults.Count % pageSize != 0) ViewBag.Pages += 1;
            if (page > ViewBag.Pages) ViewBag.Page = ViewBag.Pages;

            return filterResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        protected virtual IEnumerable<Member> GetFilteredMembersList(
            IList<Member> members, string s, string sort, string order)
        {
            IEnumerable<Member> filteredResults;

            if (!string.IsNullOrEmpty(s))
            {
                var lcs = s.ToLower();
                filteredResults = members
                    .Where(m =>
                        m.FirstName.ToLower().Contains(lcs) ||
                        m.LastName.ToLower().Contains(lcs) ||
                        m.PledgeClass.PledgeClassName.ToLower().Contains(lcs) ||
                        m.GraduationSemester.ToString().ToLower() == lcs ||
                        m.RoomString().ToLower() == lcs);
            }
            else
            {
                filteredResults = members;
            }

            switch (sort)
            {
                case "first-name":
                    filteredResults = order == "desc"
                        ? filteredResults.OrderByDescending(m => m.FirstName)
                        : filteredResults.OrderBy(m => m.FirstName);
                    break;
                case "pledge-class":
                    filteredResults = order == "desc"
                        ? filteredResults.OrderByDescending(m => m.PledgeClass.Semester.DateStart)
                        : filteredResults.OrderBy(m => m.PledgeClass.Semester.DateStart);
                    break;
                case "final-semester":
                    filteredResults = order == "desc"
                        ? filteredResults.OrderByDescending(m => m.GraduationSemester.DateStart)
                        : filteredResults.OrderBy(m => m.GraduationSemester.DateStart);
                    break;
                case "location":
                    filteredResults = order == "desc"
                        ? filteredResults.OrderByDescending(m => m.RoomString())
                        : filteredResults.OrderBy(m => m.RoomString());
                    break;
                default: // "last-name"
                    filteredResults = order == "desc"
                        ? filteredResults.OrderByDescending(m => m.LastName)
                        : filteredResults.OrderBy(m => m.LastName);
                    break;
            }

            return filteredResults;
        }

        protected virtual string ToTitleCaseString(string original)
        {
            var formattedText = string.Empty;

            var words = original.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                if (!IsAllUpper(words[i]) && !Char.IsNumber(words[i][0]))
                {
                    words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant());
                }
                formattedText += words[i];
                if (i < words.Length - 1)
                    formattedText += " ";
            }

            return formattedText;
        }
        private bool IsAllUpper(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }

        protected virtual async Task<Leader> GetCurrentLeader(string positionName)
        {
            var term = await GetCurrentTerm();
            return term.Leaders
                .Where(l => l.Position.Name == positionName)
                .OrderByDescending(l => l.AppointedOn)
                .FirstOrDefault();
        }
        protected virtual async Task<Semester> GetCurrentTerm()
        {
            return await _db.Semesters
                .Where(s => s.TransitionDate > DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .FirstOrDefaultAsync() ?? new Semester();
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        #region Messages

        public enum SoberMessage
        {
            SignupSuccess,
            SignupFailure,
            AddSignupSuccess,
            AddSignupFailure,
            MultiAddSignupSuccess,
            MultiAddSignupFailure,
            MultiAddSignupMissingTypesFailure,
            CancelSignupSuccess,
            EditSignupSuccess,
            EditSignupFailure,
            SignupPledgeOfficerFailure,
            DeleteSignupSuccess,
            MultiAddSignupNoDatesFailure,
        }
        public void SetSoberMessage(SoberMessage? message)
        {
            switch (message)
            {
                case SoberMessage.SignupSuccess:
                    ViewBag.SuccessMessage = "Sober signup was successful.";
                    break;
                case SoberMessage.AddSignupSuccess:
                    ViewBag.SuccessMessage = "Successfully added a new sober signup slot.";
                    break;
                case SoberMessage.MultiAddSignupSuccess:
                    ViewBag.SuccessMessage = "Successfully added multiple new sober signup slots.";
                    break;
                case SoberMessage.CancelSignupSuccess:
                    ViewBag.SuccessMessage = "Successfully cancelled member's sober signup.";
                    break;
                case SoberMessage.EditSignupSuccess:
                    ViewBag.SuccessMessage = "Successfully modified sober signup.";
                    break;
                case SoberMessage.DeleteSignupSuccess:
                    ViewBag.FailMessage = "Successfully deleted sober signup slot.";
                    break;
                case SoberMessage.SignupFailure:
                    ViewBag.FailMessage = "Sober signup failed.";
                    break;
                case SoberMessage.AddSignupFailure:
                    ViewBag.FailMessage = "Failed to add sober signup.  Please check the values you entered and try again.";
                    break;
                case SoberMessage.MultiAddSignupFailure:
                    ViewBag.FailMessage = "Multi-add tool failed because no amounts or dates were provided.";
                    break;
                case SoberMessage.MultiAddSignupNoDatesFailure:
                    ViewBag.FailMessage = "Multi-add tool failed because no dates were provided.";
                    break;
                case SoberMessage.MultiAddSignupMissingTypesFailure:
                    ViewBag.FailMessage = "There was an error with the multi-add tool.  " +
                              "This is most likely because the sober types Driver and Officer do not both exist.  " +
                              "If Driver and Officer types exist exactly as they are spelled here and you still " +
                              "get this message, contact Ty Morrow.";
                    break;
                case SoberMessage.EditSignupFailure:
                    ViewBag.FailMessage = "Failed to modify sober signup.  Please try again or contact your administrator" +
                                          "if the problem persists.";
                    break;
                case SoberMessage.SignupPledgeOfficerFailure:
                    ViewBag.FailMessage = "Unfortunately members with pledge status are not allowed to be sober officers.";
                    break;
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}