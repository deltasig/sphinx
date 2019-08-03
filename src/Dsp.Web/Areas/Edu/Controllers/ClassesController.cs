namespace Dsp.Web.Areas.Edu.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator")]
    public class ClassesController : BaseController
    {
        public async Task<ActionResult> Index(ClassesIndexFilterModel filter)
        {
            var classes = await _db.Classes.ToListAsync();
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
            var currentSemester = await GetThisSemesterAsync();

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
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name"),
                Class = new Class()
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateClassModel model)
        {
            var classExistsAlready = (await _db.Classes.AnyAsync(c =>
                c.CourseShorthand == model.Class.CourseShorthand && c.DepartmentId == model.Class.DepartmentId));

            if (ModelState.IsValid && !classExistsAlready)
            {
                _db.Classes.Add(model.Class);
                await _db.SaveChangesAsync();
                ViewBag.SuccessMessage = model.Class.CourseName + " class created successfully.";
                model.Class = new Class();
                model.Departments = new SelectList(await
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
            }
            else
            {
                ViewBag.FailureMessage = "Could not create new class.  Make sure the class you tried creating does not already exist.";
                model.Departments = new SelectList(await
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
            }
            return View(model);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var course = await _db.Classes.FindAsync(id);
            if (course == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var model = new ClassDetailsModel
            {
                Class = course,
                CurrentSemester = await GetThisSemesterAsync()
            };
            return View(model);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            var model = new CreateClassModel { Class = await _db.Classes.FindAsync(id) };
            if (model.Class == null) return HttpNotFound();

            model.Departments = new SelectList(await
                _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreateClassModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = new SelectList(await
                        _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
                ViewBag.FailureMessage = "The class could not be updated.  " +
                                         "Make sure the class shorthand entered does not already exist.";
                return View(model);
            }

            _db.Entry(model.Class).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Class.CourseName + " updated successfully.";
            return RedirectToAction("Details", new { id = model.Class.ClassId });

        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            var model = await _db.Classes.FindAsync(id);
            if (model == null) return HttpNotFound();

            if (!model.ClassesTaken.Any()) return View(model);

            TempData["FailureMessage"] = "Class could not be deleted because enrollments were found.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.Classes.FindAsync(id);
            if (model.ClassesTaken.Any())
            {
                TempData["FailureMessage"] = "Class could not be deleted because enrollments were found.";
                return RedirectToAction("Index");
            }
            _db.Entry(model).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.CourseName + " deleted successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Duplicates()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var shorthandGroups = await (from c in _db.Classes
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
        [Authorize(Roles = "Administrator, Academics")]
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
                    var enrollmentsToMove = await _db.ClassesTaken.Where(c => c.ClassId == cid).ToListAsync();
                    foreach (var e in enrollmentsToMove)
                    {
                        var classTaken = await _db.ClassesTaken.FindAsync(e.ClassTakenId);
                        classTaken.ClassId = primaryId;
                        _db.Entry(classTaken).State = EntityState.Modified;
                    }
                    // Now with everything moved over to the primary, remove the duplicated class.
                    var classToRemove = await _db.Classes.FindAsync(cid);
                    _db.Classes.Remove(classToRemove);
                    await _db.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "The duplicate merge was successful!";
            return RedirectToAction("Duplicates");
        }

        public async Task<ActionResult> Schedule(string userName, int? semesterId = null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var member = await UserManager.FindByNameAsync(userName);
            if (string.IsNullOrEmpty(userName) || member == null)
            {
                ViewBag.FailureMessage = "No one by that username was found!";
                userName = User.Identity.GetUserName();
            }
            member = await UserManager.FindByNameAsync(userName);

            var model = new ClassScheduleModel
            {
                SelectedUserName = userName,
                Member = member,
                AllClasses = await _db.Classes.OrderBy(c => c.CourseShorthand).ToListAsync(),
                Semesters = await GetSemesterListAsync(),
                ClassTaken = new ClassTaken
                {
                    SemesterId = semesterId == null ? (await GetThisSemesterAsync()).Id : (int)semesterId,
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

            if (!User.IsInRole("Academics") && !User.IsInRole("Administrator") &&
                User.Identity.GetUserName() != model.SelectedUserName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var member = await UserManager.FindByNameAsync(model.SelectedUserName);

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
            _db.ClassesTaken.Add(model.ClassTaken);
            await _db.SaveChangesAsync();
            var course = await _db.Classes.FindAsync(model.ClassTaken.ClassId);
            var semester = await _db.Semesters.FindAsync(model.ClassTaken.SemesterId);

            TempData["SuccessMessage"] = member + " was successfully enrolled in " + course.CourseShorthand + " for " + semester + ".";
            return RedirectToAction("Schedule", new { userName = model.SelectedUserName, semesterId = model.ClassTaken.SemesterId });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Disenroll(int ctid)
        {
            var entry = await _db.ClassesTaken.SingleAsync(c => c.ClassTakenId == ctid);
            if (entry == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            return View(entry);
        }

        [HttpPost, ActionName("Disenroll"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DisenrollConfirmed(ClassTaken model)
        {
            var entry = await _db.ClassesTaken
                .SingleAsync(c => c.UserId == model.UserId && c.SemesterId == model.SemesterId && c.ClassId == model.ClassId);
            var member = await UserManager.FindByIdAsync(model.UserId);
            var course = await _db.Classes.FindAsync(model.ClassId);
            var semester = await _db.Semesters.FindAsync(model.SemesterId);
            if (entry == null)
            {
                TempData["FailureMessage"] = "Failed to process disenrollment because no existing information was found.";
                return RedirectToAction("Schedule", new { userName = member.UserName ?? User.Identity.GetUserName() });
            }

            _db.Entry(entry).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = member + " was successfully disenrolled from " +
                course.CourseShorthand + " for " + semester + ".";
            return RedirectToAction("Schedule", new { userName = member.UserName ?? User.Identity.GetUserName() });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditEnrollment(int ctid)
        {
            var enrollment = await _db.ClassesTaken.SingleAsync(c => c.ClassTakenId == ctid);
            if (enrollment == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new EditEnrollmentModel
            {
                Enrollment = enrollment
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditEnrollment(EditEnrollmentModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Failed to update enrollment information.";
                return RedirectToAction("Schedule", new { userName = model.Enrollment.Member.UserName });
            }

            var classTaken = await _db.ClassesTaken
                    .SingleAsync(t =>
                        t.ClassId == model.Enrollment.ClassId &&
                        t.SemesterId == model.Enrollment.SemesterId &&
                        t.UserId == model.Enrollment.UserId);
            classTaken.IsSummerClass = model.Enrollment.IsSummerClass;
            _db.Entry(classTaken).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var member = await UserManager.FindByIdAsync(model.Enrollment.UserId);

            TempData["SuccessMessage"] = "Enrollment update was successful.";
            return RedirectToAction("Schedule", new { userName = member.UserName });
        }
    }
}
