namespace Dsp.WebCore.Areas.School.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

[Area("School")]
[Authorize]
public class ClassesController : BaseController
{
    private readonly IPositionService _positionService;
    private readonly IMemberService _memberService;
    private readonly ISemesterService _semesterService;

    public ClassesController(IPositionService positionService, IMemberService memberService, ISemesterService semesterService)
    {
        _positionService = positionService;
        _memberService = memberService;
        _semesterService = semesterService;
    }

    public async Task<ActionResult> Index(ClassesIndexFilterModel filter)
    {
        var classes = await Context.Classes.ToListAsync();
        var filterResults = await GetFilteredClassList(
            classes,
            filter.s,
            filter.sort,
            filter.page,
            filter.select,
            filter.pageSize
        );
        var totalPages = ViewBag.Pages;
        var resultCount = ViewBag.Count;
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();

        var model = new ClassIndexModel(
            filterResults,
            currentSemester,
            filter,
            totalPages,
            resultCount
        );

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        return View(model);
    }

    public async Task<ActionResult> Create()
    {
        var model = new CreateClassModel
        {
            Departments = new SelectList(await
                Context.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name"),
            Class = new Class()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(CreateClassModel model)
    {
        var classExistsAlready = (await Context.Classes.AnyAsync(c =>
            c.CourseShorthand == model.Class.CourseShorthand && c.DepartmentId == model.Class.DepartmentId));

        if (ModelState.IsValid && !classExistsAlready)
        {
            Context.Classes.Add(model.Class);
            await Context.SaveChangesAsync();
            ViewBag.SuccessMessage = model.Class.CourseName + " class created successfully.";
            model.Class = new Class();
            model.Departments = new SelectList(await
                Context.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
        }
        else
        {
            ViewBag.FailureMessage = "Could not create new class.  Make sure the class you tried creating does not already exist.";
            model.Departments = new SelectList(await
                Context.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
        }
        return View(model);
    }

    public async Task<ActionResult> Details(int? id)
    {
        if (id == null) return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        var course = await Context.Classes
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.ClassId == id);
        if (course == null) return NotFound();

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        var model = new ClassDetailsModel
        {
            Class = course,
            CurrentSemester = await _semesterService.GetCurrentSemesterAsync()
        };
        return View(model);
    }

    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int)HttpStatusCode.NotFound);
        var model = new CreateClassModel { Class = await Context.Classes.FindAsync(id) };
        if (model.Class == null) return NotFound();

        model.Departments = new SelectList(await
            Context.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(CreateClassModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Departments = new SelectList(await
                    Context.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
            ViewBag.FailureMessage = "The class could not be updated.  " +
                                     "Make sure the class shorthand entered does not already exist.";
            return View(model);
        }

        Context.Entry(model.Class).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = model.Class.CourseName + " updated successfully.";
        return RedirectToAction("Details", new { id = model.Class.ClassId });

    }

    [Authorize]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null) return new StatusCodeResult((int)HttpStatusCode.NotFound);
        var model = await Context.Classes.FindAsync(id);
        if (model == null) return NotFound();

        if (!model.ClassesTaken.Any()) return View(model);

        TempData["FailureMessage"] = "Class could not be deleted because enrollments were found.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var model = await Context.Classes.FindAsync(id);
        if (model.ClassesTaken.Any())
        {
            TempData["FailureMessage"] = "Class could not be deleted because enrollments were found.";
            return RedirectToAction("Index");
        }
        Context.Entry(model).State = EntityState.Deleted;
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = model.CourseName + " deleted successfully.";
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<ActionResult> Duplicates()
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        var shorthandGroups = await (from c in Context.Classes
                                     group c by c.CourseShorthand.ToLower() into g
                                     select new DuplicateGroup
                                     {
                                         Shorthand = g.Key,
                                         Classes = g.Select(n => new DuplicateClass
                                         {
                                             Class = n,
                                             IsPrimary = false
                                         }).ToList()
                                     }).ToListAsync();

        var model = shorthandGroups.Where(g => g.Classes.Count > 1).ToList();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> Duplicates(List<DuplicateGroup> model)
    {
        // If no primary was selected, return an error message.
        if (!model.SelectMany(g => g.Classes).Any(c => c.IsPrimary))
        {
            TempData["FailureMessage"] = "Nothing was merged because no primary class was selected.";
            return RedirectToAction("Duplicates");
        }

        foreach (var group in model)
        {
            // Skip if they did not check only one box.
            if (group.Classes.Count(c => c.IsPrimary) != 1) continue;

            // Start the merging procedure.
            var primaryId = group.Classes.Single(c => c.IsPrimary).Class.ClassId;
            // Remove the primary group from the model list so that it doesn't get include in following loop.
            group.Classes.Remove(group.Classes.Single(c => c.IsPrimary));

            foreach (var cid in group.Classes.Select(cl => cl.Class.ClassId))
            {
                // Set any classTakens (enrollments) to use the primary class.
                var enrollmentsToMove = await Context.ClassesTaken.Where(c => c.ClassId == cid).ToListAsync();
                foreach (var e in enrollmentsToMove)
                {
                    var classTaken = await Context.ClassesTaken.FindAsync(e.ClassTakenId);
                    classTaken.ClassId = primaryId;
                    Context.Entry(classTaken).State = EntityState.Modified;
                }
                // Now with everything moved over to the primary, remove the duplicated class.
                var classToRemove = await Context.Classes.FindAsync(cid);
                Context.Classes.Remove(classToRemove);
                await Context.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = "The duplicate merge was successful!";
        return RedirectToAction("Duplicates");
    }

    public async Task<ActionResult> Schedule(string userName, int? semesterId = null)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        var member = await _memberService.GetMemberByUserNameAsync(userName);
        if (string.IsNullOrEmpty(userName) || member == null)
        {
            ViewBag.FailureMessage = "No one by that username was found!";
            userName = User.GetUserName();
        }
        member = await _memberService.GetMemberByUserNameAsync(userName);

        var model = new ClassScheduleModel
        {
            SelectedUserName = userName,
            User = member,
            AllClasses = await Context.Classes.OrderBy(c => c.CourseShorthand).ToListAsync(),
            Semesters = (await _semesterService.GetAllSemestersAsync()).ToSelectList(),
            ClassTaken = new ClassTaken
            {
                SemesterId = semesterId == null ? (await _semesterService.GetCurrentSemesterAsync()).Id : (int)semesterId,
                UserId = member.Id
            },
            ClassesTaken = member.ClassesTaken
        };

        ViewBag.UserName = userName;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Enroll(ClassScheduleModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["FailureMessage"] = "Failed to add enrollment information for an unknown reason.  " +
                                         "Contact your administrator if the problem persists.";
            return RedirectToAction("Schedule", new { userName = model.SelectedUserName });
        }

        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Academics");
        if (!hasElevatedPermissions && User.GetUserName() != model.SelectedUserName)
        {
            return new StatusCodeResult((int)HttpStatusCode.NotFound);
        }
        var member = await _memberService.GetMemberByUserNameAsync(model.SelectedUserName);

        if (member.ClassesTaken.Any(c =>
            c.ClassId == model.ClassTaken.ClassId &&
            c.UserId == model.ClassTaken.UserId &&
            c.SemesterId == model.ClassTaken.SemesterId))
        {
            TempData["FailureMessage"] = "Failed to enroll in class.  " +
                                         "Enrollment in the same class multiple times for a given semester is not allowed.";
            return RedirectToAction("Schedule", new { userName = model.SelectedUserName, semesterId = model.ClassTaken.SemesterId });
        }

        model.ClassTaken.CreatedOn = DateTime.UtcNow;
        Context.ClassesTaken.Add(model.ClassTaken);
        await Context.SaveChangesAsync();
        var course = await Context.Classes.FindAsync(model.ClassTaken.ClassId);
        var semester = await Context.Semesters.FindAsync(model.ClassTaken.SemesterId);

        TempData["SuccessMessage"] = member + " was successfully enrolled in " + course.CourseShorthand + " for " + semester + ".";
        return RedirectToAction("Schedule", new { userName = model.SelectedUserName, semesterId = model.ClassTaken.SemesterId });
    }

    [Authorize]
    public async Task<ActionResult> Disenroll(int ctid)
    {
        var entry = await Context.ClassesTaken.SingleAsync(c => c.ClassTakenId == ctid);
        if (entry == null) return new StatusCodeResult((int)HttpStatusCode.NotFound);
        return View(entry);
    }

    [HttpPost, ActionName("Disenroll"), ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> DisenrollConfirmed(ClassTaken model)
    {
        var entry = await Context.ClassesTaken
            .SingleAsync(c => c.UserId == model.UserId && c.SemesterId == model.SemesterId && c.ClassId == model.ClassId);
        var member = await _memberService.GetMemberByIdAsync(model.UserId);
        var course = await Context.Classes.FindAsync(model.ClassId);
        var semester = await Context.Semesters.FindAsync(model.SemesterId);
        if (entry == null)
        {
            TempData["FailureMessage"] = "Failed to process disenrollment because no existing information was found.";
            return RedirectToAction("Schedule", new { userName = member.UserInfo.UserName ?? User.GetUserName() });
        }

        Context.Entry(entry).State = EntityState.Deleted;
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = member + " was successfully disenrolled from " +
            course.CourseShorthand + " for " + semester + ".";
        return RedirectToAction("Schedule", new { userName = member.UserInfo.UserName ?? User.GetUserName() });
    }

    [Authorize]
    public async Task<ActionResult> EditEnrollment(int ctid)
    {
        var enrollment = await Context.ClassesTaken.SingleAsync(c => c.ClassTakenId == ctid);
        if (enrollment == null) return new StatusCodeResult((int)HttpStatusCode.NotFound);

        var model = new EditEnrollmentModel
        {
            Enrollment = enrollment
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> EditEnrollment(EditEnrollmentModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["FailureMessage"] = "Failed to update enrollment information.";
            return RedirectToAction("Schedule", new { userName = model.Enrollment.User.UserInfo.UserName });
        }

        var classTaken = await Context.ClassesTaken
                .SingleAsync(t =>
                    t.ClassId == model.Enrollment.ClassId &&
                    t.SemesterId == model.Enrollment.SemesterId &&
                    t.UserId == model.Enrollment.UserId);
        classTaken.IsSummerClass = model.Enrollment.IsSummerClass;
        Context.Entry(classTaken).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        var member = await _memberService.GetMemberByUserNameAsync(model.Enrollment.UserId.ToString());

        TempData["SuccessMessage"] = "Enrollment update was successful.";
        return RedirectToAction("Schedule", new { userName = member.UserInfo.UserName });
    }

    protected virtual async Task<IEnumerable<Class>> GetFilteredClassList(
        IList<Class> classes,
        string s, string sort, int page, string select, int pageSize = 20)
    {
        var filterResults = classes;
        var thisSemester = await _semesterService.GetCurrentSemesterAsync();
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
}
