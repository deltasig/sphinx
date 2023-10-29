namespace Dsp.WebCore.Controllers;

using Areas.Scholarships.Models;
using Dsp.Data;
using Dsp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

public class BaseController : Controller
{
    protected const string SuccessMessageKey = "SuccessMessage";
    protected const string FailureMessageKey = "FailureMessage";

    private DspDbContext _context;
    private UserManager<User> _userManager;
    private SignInManager<User> _signInManager;
    private RoleManager<Role> _roleManager;

    protected DspDbContext Context => _context ??= HttpContext.RequestServices.GetService<DspDbContext>();
    protected UserManager<User> UserManager => _userManager ??= HttpContext.RequestServices.GetService<UserManager<User>>();
    protected SignInManager<User> SignInManager => _signInManager ??= HttpContext.RequestServices.GetService<SignInManager<User>>();
    protected RoleManager<Role> RoleManager => _roleManager ??= HttpContext.RequestServices.GetService<RoleManager<Role>>();

    protected virtual async Task<IEnumerable<User>> GetAllNewMembersAsync()
    {
        return await Context.Users.Where(m => m.Status.StatusName == "New").ToListAsync();
    }
    protected virtual async Task<IEnumerable<User>> GetAllActiveNewNeophyteMembersAsync()
    {
        return await Context.Users
            .Where(m => m.Status.StatusName == "Active" ||
                        m.Status.StatusName == "New" ||
                        m.Status.StatusName == "Neophyte")
            .ToListAsync();
    }
    protected virtual async Task<List<User>> GetRosterForSemester(Semester semester)
    {
        return await Context.Users
            .Where(d =>
                d.LastName != "Hirtz" &&
                (d.Status.StatusName == "Alumnus" ||
                d.Status.StatusName == "Active" ||
                d.Status.StatusName == "New" ||
                d.Status.StatusName == "Neophyte") &&
                d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                d.ExpectedGraduation.DateEnd > semester.DateStart)
            .Include(m => m.PledgeClass)
            .Include(m => m.Status)
            .ToListAsync();
    }
    protected virtual async Task<Semester> GetThisSemesterAsync()
    {
        return (await Context.Semesters
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .First();
    }
    protected virtual async Task<Semester> GetLastSemesterAsync()
    {
        return (await Context.Semesters
                .Where(s => s.DateEnd < DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .Last();
    }
    protected virtual async Task<IEnumerable<object>> GetUserIdListAsFullNameWithNoneNonSelectListAsync()
    {
        var members = (await GetAllActiveNewNeophyteMembersAsync()).OrderBy(o => o.LastName);
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
        var members = (await Context.Users.ToListAsync()).OrderBy(o => o.LastName);
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
    protected virtual async Task<SelectList> GetAllSemesterListAsync()
    {
        var semesters = await Context.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
        var newList = new List<object>();

        foreach (var s in semesters)
        {
            newList.Add(new
            {
                s.Id,
                Name = s.ToString()
            });
        }

        return new SelectList(newList, "Id", "Name", (await GetThisSemesterAsync()).Id);
    }
    protected virtual async Task<SelectList> GetAllSemesterListWithNoneAsync()
    {
        var semesters = await Context.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
        var newList = new List<object> { new { Id = 0, Name = "None" } };

        foreach (var s in semesters)
        {
            newList.Add(new
            {
                s.Id,
                Name = s.ToString()
            });
        }

        return new SelectList(newList, "Id", "Name");
    }
    protected virtual async Task<SelectList> GetSemesterListAsync()
    {
        var currentSemester = await GetThisSemesterAsync();
        var semesters = await Context.Semesters
            .Where(s => s.DateEnd <= currentSemester.DateEnd)
            .OrderByDescending(s => s.DateEnd).ToListAsync();
        var newList = new List<object>();

        foreach (var s in semesters)
        {
            newList.Add(new
            {
                s.Id,
                Name = s.ToString()
            });
        }

        return new SelectList(newList, "Id", "Name", (await GetThisSemesterAsync()).Id);
    }
    protected virtual SelectList GetSemesterSelectList(IEnumerable<Semester> list)
    {
        var newList = new List<object>();

        foreach (var s in list.OrderByDescending(s => s.DateEnd))
        {
            newList.Add(new
            {
                s.Id,
                Name = s.ToString()
            });
        }

        return new SelectList(newList, "Id", "Name");
    }
    protected virtual async Task<SelectList> GetUserIdListAsFullNameAsync()
    {
        var members = (await GetAllActiveNewNeophyteMembersAsync()).OrderBy(o => o.LastName);
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
    protected virtual async Task<SelectList> GetUsersAsFullNameAsync(Expression<Func<User, bool>> preSelector, Func<User, bool> postSelector)
    {
        var users = await Context.Users
            .Where(preSelector)
            .OrderBy(u => u.LastName)
            .ToListAsync();

        var newList = new List<object>();
        foreach (var u in users.Where(postSelector))
        {
            newList.Add(new
            {
                UserId = u.Id,
                Name = $"{u.FirstName} {u.LastName}"
            });
        }
        return new SelectList(newList, "UserId", "Name");
    }
    protected virtual async Task<SelectList> GetNewMemberUserIdListAsFullNameAsync()
    {
        var members = (await GetAllNewMembersAsync()).OrderBy(o => o.LastName);
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
        var thisAndPreviousSemesters = await Context.Semesters
            .Where(s => s.DateEnd <= DateTime.UtcNow)
            .ToListAsync();
        var semesterIds = thisAndPreviousSemesters.Select(s => (int?)s.Id);
        var members = UserManager.Users
            .Where(m =>
                (m.ExpectedGraduationId == null || semesterIds.Contains(m.ExpectedGraduationId))
                && m.Status.StatusName == "Active")
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
        var statusList = await Context.UserTypes.ToListAsync();
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
        var statusList = await Context.UserTypes.ToListAsync();
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
        var pledgeClasses = await Context.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
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
        var pledgeClasses = await Context.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
        var newList = new List<object> { new { PledgeClassId = 0, PledgeClassName = "None" } };

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
    protected virtual async Task<double> GetRemainingServiceHoursForUserAsync(int userId)
    {
        const double requiredHours = 10;

        var lastSemester = await GetLastSemesterAsync();
        var currentSemester = await GetThisSemesterAsync();

        var totalHours = 0.0;
        try
        {
            var serviceHours = await Context.ServiceHours
                .Where(h => h.User.Id == userId &&
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
    // TODO: Remove
    protected virtual async Task<IEnumerable<ServiceHour>> GetAllCompletedEventsForUserAsync(int userId)
    {
        var thisSemester = await GetThisSemesterAsync();
        var lastSemester = await GetLastSemesterAsync();

        return await Context.ServiceHours
            .Where(e => e.UserId == userId &&
                        e.Event.DateTimeOccurred > lastSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= thisSemester.DateEnd)
            .ToListAsync();
    }
    protected virtual async Task<IEnumerable<SoberSignup>> GetSoberSignupsForUserAsync(int userId, Semester semester)
    {
        var previousSemester = (await Context.Semesters
            .Where(s => s.DateEnd < semester.DateStart)
            .OrderBy(s => s.DateStart)
            .ToListAsync())
            .Last();

        return await Context.SoberSignups
            .Where(s => s.UserId == userId &&
                        s.DateOfShift > previousSemester.DateEnd &&
                        s.DateOfShift <= semester.DateEnd)
            .ToListAsync();
    }
    protected virtual async Task<SelectList> GetScholarshipTypesSelectListAsync()
    {
        var types = await Context.ScholarshipTypes.ToListAsync();
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
        var questions = await Context.ScholarshipQuestions.ToListAsync();
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
    protected virtual IEnumerable<WorkOrder> GetFilteredWorkOrderList(
        IList<WorkOrder> workOrders,
        string s, string sort, int page, bool open, bool closed, int pageSize = 10)
    {
        var filterResults = new List<WorkOrder>();
        // Filter out results based on open, closed, sort, and/or search string.
        if (open)
        {
            var openResults = workOrders.Where(w => w.IsOpen).ToList();
            filterResults.AddRange(openResults);
        }
        if (closed)
        {
            var closedResults = workOrders.Where(w => w.IsClosed).ToList();
            filterResults.AddRange(closedResults);
        }
        switch (sort)
        {
            case "oldest":
                filterResults = filterResults.OrderBy(o => o.CreatedOn).ToList();
                break;
            default:
                sort = "newest";
                filterResults = filterResults.OrderByDescending(o => o.CreatedOn).ToList();
                break;
        }
        if (!string.IsNullOrEmpty(s))
        {
            s = s.ToLower();
            filterResults = filterResults
                .Where(w =>
                    w.WorkOrderId.ToString() == s ||
                    w.Title.ToLower().Contains(s) ||
                    w.User.FirstName.ToLower().Contains(s) ||
                    w.User.LastName.ToLower().Contains(s))
                .ToList();
        }
        ViewBag.OpenResultCount = filterResults.Count(w => w.IsOpen);
        ViewBag.ClosedResultCount = filterResults.Count(w => w.IsClosed);

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
            filterResults = filterResults
                .Where(i =>
                    i.PolicyBroken.ToLower().Contains(s) ||
                    i.OfficialReport.ToLower().Contains(s))
                .ToList();

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
        var currentUser = await UserManager.GetUserAsync(HttpContext.User);
        var userId = currentUser.Id;
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
                        t.SemesterId == thisSemester.Id &&
                        !t.IsSummerClass)).ToList();
                break;
            case "being-taken":
                filterResults = classes.Where(c =>
                    c.ClassesTaken.Any(t => t.SemesterId == thisSemester.Id)).ToList();
                break;
            case "none-taking":
                filterResults = classes.Where(c =>
                    c.ClassesTaken.All(t => t.SemesterId != thisSemester.Id)).ToList();
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
            case "taken-asc":
                filterResults = filterResults.OrderBy(o => o.ClassesTaken.Count).ThenBy(o => o.CourseShorthand).ToList();
                break;
            case "taken-desc":
                filterResults = filterResults.OrderByDescending(o => o.ClassesTaken.Count).ThenBy(o => o.CourseShorthand).ToList();
                break;
            case "taking-asc":
                filterResults = filterResults.OrderBy(o =>
                        o.ClassesTaken.Select(t => t.SemesterId == thisSemester.Id).ToList().Count)
                    .ThenBy(o => o.CourseShorthand).ToList();
                break;
            case "taking-desc":
                filterResults = filterResults.OrderByDescending(o =>
                        o.ClassesTaken.Select(t => t.SemesterId == thisSemester.Id).ToList().Count)
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
                    i.ClassesTaken.Any(t => t.User.ToString().ToLower().Contains(s)))
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

    protected virtual IEnumerable<User> GetFilteredMembersList(
        IList<User> members, string s, string sort, string order)
    {
        IEnumerable<User> filteredResults;

        if (!string.IsNullOrEmpty(s))
        {
            var lcs = s.ToLower();
            filteredResults = members
                .Where(m =>
                    m.FirstName.ToLower().Contains(lcs) ||
                    m.LastName.ToLower().Contains(lcs) ||
                    m.PledgeClass.PledgeClassName.ToLower().Contains(lcs) ||
                    m.ExpectedGraduation.ToString().ToLower() == lcs ||
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
                    ? filteredResults.OrderByDescending(m => m.ExpectedGraduation.DateStart)
                    : filteredResults.OrderBy(m => m.ExpectedGraduation.DateStart);
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

    protected virtual SelectList GetShirtSizesSelectList()
    {
        var newList = new List<string> { "S", "M", "L", "XL", "2XL" };
        var list = new SelectList(newList.Select(x => new { Value = x, Text = x }), "Value", "Text");
        return list;
    }
    protected virtual async Task<UserRole> GetCurrentLeader(string positionName)
    {
        var term = await GetCurrentTerm();
        return term.Leaders
            .Where(l => l.Role.Name == positionName)
            .OrderByDescending(l => l.AppointedOn)
            .FirstOrDefault();
    }
    protected virtual async Task<Semester> GetCurrentTerm()
    {
        return await Context.Semesters
            .Where(s => s.TransitionDate > DateTime.UtcNow)
            .OrderBy(s => s.DateStart)
            .FirstOrDefaultAsync() ?? new Semester();
    }

    protected string RenderRazorViewToString(string viewName, object model)
    {
        ViewData.Model = model;
        using (var sw = new StringWriter())
        {
            IViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);
            var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());
            viewResult.View.RenderAsync(viewContext);
            return sw.GetStringBuilder().ToString();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Dispose();
        }
        base.Dispose(disposing);
    }
}