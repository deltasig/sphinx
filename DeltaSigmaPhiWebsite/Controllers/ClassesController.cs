namespace DeltaSigmaPhiWebsite.Controllers
{
    using Models.Entities;
    using Models.ViewModels;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    
    [Authorize]
    public class ClassesController : BaseController
    {
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Index(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }
            var classes = _db.Classes.OrderBy(c => c.CourseShorthand).ToList();
            return View(classes);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Create()
        {
            ViewBag.Message = string.Empty;
            ViewBag.DepartmentId = new SelectList(
                _db.Departments.ToList().OrderBy(c => c.DepartmentName), 
                "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Create([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName,CreditHours")] Class @class)
        {
            ViewBag.Message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_db.Classes.ToList()
                    .Any(c => c.CourseShorthand == @class.CourseShorthand && 
                        c.DepartmentId == @class.DepartmentId))
                {
                    ViewBag.Message = "A Class in that department with that number already exists.";
                }
                else
                {
                    _db.Classes.Add(@class);
                    _db.SaveChanges();
                    ViewBag.Message = "Class created successfully. ";
                }
            }

            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            Class @class = _db.Classes.Find(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(
                _db.Departments.ToList().OrderBy(c => c.DepartmentName), 
                "DepartmentId", "DepartmentName", 
                @class.DepartmentId);
            return View(@class);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Edit([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName,CreditHours")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(@class).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(_db.Departments, "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var @class = _db.Classes.Find(id);
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
        public ActionResult DeleteConfirmed(int id)
        {
            var @class = _db.Classes.Find(id);
            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = "This class is currently being taken, or has been taken, " +
                              "by one or more people, therefore it can't be deleted."
                });
            }
            _db.Entry(@class).State = EntityState.Deleted;
            _db.SaveChanges();
            return RedirectToAction("Index", new { message = "Course deleted." });
        }

        public ActionResult CreateSchedule()
        {
            var model = new ClassScheduleModel
            {
                Members = GetUserIdListAsFullName(),
                AllClasses = _db.Classes.OrderBy(c => c.CourseShorthand).ToList(),
                Semesters = GetSemesterList(),
                SelectedSemester = GetThisSemester().SemesterId,
                ClassesTaken = new List<ClassTaken> { new ClassTaken() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult CreateSchedule(ClassScheduleModel model)
        {
            var message = "";
            var duplicateCount = 0;
            if(ModelState.IsValid)
            {
                foreach(var course in model.ClassesTaken)
                {
                    var classesTaken = _db.ClassesTaken.Where(c => 
                            c.ClassId == course.ClassId && 
                            c.SemesterId == model.SelectedSemester && 
                            c.UserId == model.SelectedMember)
                        .ToList();
                    if (classesTaken.Any())
                    {
                        duplicateCount++;
                        continue;
                    }

                    course.SemesterId = model.SelectedSemester;
                    course.UserId = model.SelectedMember;
                    _db.ClassesTaken.Add(course);
                }
                _db.SaveChanges();
                if(duplicateCount < model.ClassesTaken.Count)
                {
                    message = "Class(es) added successfully. ";
                }
                if (duplicateCount > 0)
                {
                    message += duplicateCount + " class(es) were not added since the member was already enrolled.";
                }
            }
            var member = _db.Members.Find(model.SelectedMember);
            return RedirectToAction("Schedule", new { userName = member.UserName, message});
        }

        public ActionResult Schedule(string userName, string message)
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
            var model = _db.ClassesTaken
                .Where(c => c.Member.UserName == userName)
                .OrderByDescending(c => c.SemesterId)
                .ToList();
            if(model.Count <= 0)
            {
                ViewBag.Message += "No classes found for " + userName + ". ";
            }

            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult DeleteFromSchedule(int id, int sid, int cid, string username)
        {
            var entry = _db.ClassesTaken.Single(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (entry == null)
            {
                return RedirectToAction("Schedule", new { userName = username, message = "Course not found. " });
            }

            _db.Entry(entry).State = EntityState.Deleted;
            _db.SaveChanges();

            return RedirectToAction("Schedule", new { userName = username, message = "Course deleted from schedule. " });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult EditClassTaken(int id, int sid, int cid)
        {
            var model = _db.ClassesTaken.Single(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult EditClassTaken([Bind(Include = "UserId,ClassId,SemesterId,Instructor,MidtermGrade,FinalGrade,Dropped")] ClassTaken classTaken)
        {
            if (!ModelState.IsValid) 
                return RedirectToAction("Schedule", new {userName = classTaken.Member.UserName, message = "Failed to update record."});

            _db.Entry(classTaken).State = EntityState.Modified;
            _db.SaveChanges();

            var member = _db.Members.Find(classTaken.UserId);

            return RedirectToAction("Schedule", new { userName = member.UserName, message = "Record updated." });
        }
    }
}
