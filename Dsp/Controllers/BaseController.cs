﻿namespace Dsp.Controllers
{
    using Areas.Scholarships.Models;
    using Data;
    using Entities;
    using Extensions;
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
                    (d.MemberStatus.StatusName == "Active" || 
                    d.MemberStatus.StatusName == "Pledge" || 
                    d.MemberStatus.StatusName == "Neophyte") &&
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
        protected virtual async Task<SelectList> GetCustomSemesterListAsync(IEnumerable<Semester> list)
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
            var statusList = await _db.MemberStatus.ToListAsync();
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
            var statusList = await _db.MemberStatus.ToListAsync();
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

            var events = await _db.Events
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
            foreach(var g in gradeList)
            {
                list.Add(new { Value = g, Text = g } );
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
                    m.MealItemId,
                    Text = m.Name
                });
            }
            return new SelectList(newList, "MealItemId", "Text");
        }
        protected virtual async Task<SelectList> GetMealsSelectListAsync()
        {
            var list = await _db.Meals.ToListAsync();
            var newList = new List<object>();
            foreach (var m in list)
            {
                newList.Add(new
                {
                    m.MealId,
                    Text = m.ToString()
                });
            }
            return new SelectList(newList, "MealId", "Text");
        }
        protected virtual async Task<SelectList> GetMealsSelectListWithNoneAsync()
        {
            var list = await _db.Meals.ToListAsync();
            var newList = new List<object> { new { MealId = -1, Text = "None" } };
            foreach (var m in list)
            {
                newList.Add(new
                {
                    m.MealId,
                    Text = m.ToString()
                });
            }
            return new SelectList(newList, "MealId", "Text");
        }

        protected virtual async Task<SelectList> GetSoberTypesSelectList()
        {
            var list = await _db.SoberTypes.ToListAsync();
            var newList = new List<object>();
            foreach (var m in list)
            {
                newList.Add(new
                {
                    m.SoberTypeId,
                    Text = m.Name
                });
            }
            return new SelectList(newList, "SoberTypeId", "Text");
        }

        protected virtual string ToTitleCaseString(string original)
        {
            var formattedText = string.Empty;

            var words = original.Split(' ');
            for(var i = 0; i < words.Length; i++)
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