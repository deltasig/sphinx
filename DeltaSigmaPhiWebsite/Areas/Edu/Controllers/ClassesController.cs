namespace DeltaSigmaPhiWebsite.Areas.Edu.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using DeltaSigmaPhiWebsite.Entities;
    using Models;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize]
    public class ClassesController : BaseController
    {
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Index(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }
            var classes = await _db.Classes.OrderBy(c => c.CourseShorthand).ToListAsync();
            return View(classes);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Create()
        {
            ViewBag.Message = string.Empty;
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.DepartmentName).ToListAsync(), 
                "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Create([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName,CreditHours")] Class @class)
        {
            ViewBag.Message = string.Empty;
            if (ModelState.IsValid)
            {
                if (await _db.Classes.AnyAsync(c => c.CourseShorthand == @class.CourseShorthand && c.DepartmentId == @class.DepartmentId))
                {
                    ViewBag.Message = "A Class in that department with that number already exists.";
                }
                else
                {
                    _db.Classes.Add(@class);
                    await _db.SaveChangesAsync();
                    ViewBag.Message = "Class created successfully. ";
                }
            }

            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var @class = await _db.Classes.FindAsync(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.DepartmentName).ToListAsync(), 
                "DepartmentId", "DepartmentName", 
                @class.DepartmentId);
            return View(@class);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName,CreditHours")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(@class).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var @class = await _db.Classes.FindAsync(id);
            if (@class == null)
            {
                return HttpNotFound();
            }

            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = "This class is currently being taken, or has been taken, " +
                              "by one or more people, therefore it can't be deleted."
                });
            }

            return View(@class);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var @class = await _db.Classes.FindAsync(id);
            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = "This class is currently being taken, or has been taken, " +
                              "by one or more people, therefore it can't be deleted."
                });
            }
            _db.Entry(@class).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { message = "Course deleted." });
        }

        public async Task<ActionResult> CreateSchedule()
        {
            var model = new ClassScheduleModel
            {
                Members = await GetUserIdListAsFullNameAsync(),
                AllClasses = await _db.Classes.OrderBy(c => c.CourseShorthand).ToListAsync(),
                Semesters = await GetSemesterListAsync(),
                SelectedSemester = (await GetThisSemesterAsync()).SemesterId,
                ClassesTaken = new List<ClassTaken> { new ClassTaken() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> CreateSchedule(ClassScheduleModel model)
        {
            var message = "";
            var duplicateCount = 0;
            if(ModelState.IsValid)
            {
                foreach(var course in model.ClassesTaken)
                {
                    var classesTaken = await _db.ClassesTaken
                        .Where(c => c.ClassId == course.ClassId && 
                                    c.SemesterId == model.SelectedSemester && 
                                    c.UserId == model.SelectedMember)
                        .ToListAsync();
                    if (classesTaken.Any())
                    {
                        duplicateCount++;
                        continue;
                    }

                    course.SemesterId = model.SelectedSemester;
                    course.UserId = model.SelectedMember;
                    _db.ClassesTaken.Add(course);
                }
                await _db.SaveChangesAsync();
                if(duplicateCount < model.ClassesTaken.Count)
                {
                    message = "Class(es) added successfully. ";
                }
                if (duplicateCount > 0)
                {
                    message += duplicateCount + " class(es) were not added since the member was already enrolled.";
                }
            }
            var member = await _db.Members.FindAsync(model.SelectedMember);
            return RedirectToAction("Schedule", new { userName = member.UserName, message});
        }

        public async Task<ActionResult> Schedule(string userName, string message)
        {
            ViewBag.Message = string.Empty;

            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.Message = "Couldn't find anyone by that user name. ";
                return View(new List<ClassTaken>());
            }
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }
            var model = await _db.ClassesTaken
                .Where(c => c.Member.UserName == userName)
                .OrderByDescending(c => c.SemesterId)
                .ToListAsync();
            if(model.Count <= 0)
            {
                ViewBag.Message += "No classes found for " + userName + ". ";
            }

            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteFromSchedule(int id, int sid, int cid, string username)
        {
            var entry = await _db.ClassesTaken.SingleAsync(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (entry == null)
            {
                return RedirectToAction("Schedule", new { userName = username, message = "Course not found. " });
            }

            _db.Entry(entry).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { userName = username, message = "Course deleted from schedule. " });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditClassTaken(int id, int sid, int cid)
        {
            var model = await _db.ClassesTaken.SingleAsync(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditClassTaken([Bind(Include = "UserId,ClassId,SemesterId,Instructor,MidtermGrade,FinalGrade,Dropped")] ClassTaken classTaken)
        {
            if (!ModelState.IsValid) 
                return RedirectToAction("Schedule", new {userName = classTaken.Member.UserName, message = "Failed to update record."});

            _db.Entry(classTaken).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var member = await _db.Members.FindAsync(classTaken.UserId);

            return RedirectToAction("Schedule", new { userName = member.UserName, message = "Record updated." });
        }
    }
}
