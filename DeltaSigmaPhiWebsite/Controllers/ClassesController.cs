namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    
    [Authorize]
    public class ClassesController : BaseController
    {
        public ClassesController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }


        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Index(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }
            var classes = uow.ClassesRepository.SelectAll().OrderBy(c => c.CourseShorthand).ToList();
            return View(classes);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Create()
        {
            ViewBag.Message = string.Empty;
            ViewBag.DepartmentId = new SelectList(
                uow.DepartmentsRepository.SelectAll().ToList().OrderBy(c => c.DepartmentName), 
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
                if (uow.ClassesRepository.SelectAll().ToList()
                    .Any(c => c.CourseShorthand == @class.CourseShorthand && 
                        c.DepartmentId == @class.DepartmentId))
                {
                    ViewBag.Message = "A Class in that department with that number already exists.";
                }
                else
                {
                    uow.ClassesRepository.Insert(@class);
                    uow.Save();
                    ViewBag.Message = "Class created successfully. ";
                }
            }

            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            Class @class = uow.ClassesRepository.SingleById(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(
                uow.DepartmentsRepository.SelectAll().ToList().OrderBy(c => c.DepartmentName), 
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
                uow.ClassesRepository.Update(@class);
                uow.Save();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var @class = uow.ClassesRepository.SingleById(id);
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
            var @class = uow.ClassesRepository.SingleById(id);
            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = "This class is currently being taken, or has been taken, " +
                              "by one or more people, therefore it can't be deleted."
                });
            }
            uow.ClassesRepository.DeleteById(id);
            uow.Save();
            return RedirectToAction("Index", new { message = "Course deleted." });
        }

        public ActionResult CreateSchedule()
        {
            var model = new ClassScheduleModel
            {
                Members = GetUserIdListAsFullName(),
                AllClasses = uow.ClassesRepository.SelectAll().OrderBy(c => c.CourseShorthand).ToList(),
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
                    var classesTaken = uow.ClassesTakenRepository.SelectBy(c => 
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
                    uow.ClassesTakenRepository.Insert(course);
                }
                uow.Save();
                if(duplicateCount < model.ClassesTaken.Count)
                {
                    message = "Class(es) added successfully. ";
                }
                if (duplicateCount > 0)
                {
                    message += duplicateCount + " class(es) were not added since the member was already enrolled.";
                }
            }
            var member = uow.MemberRepository.SingleById(model.SelectedMember);
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
            var model = uow.ClassesTakenRepository
                .SelectBy(c => c.Member.UserName == userName)
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
            var entry = uow.ClassesTakenRepository.Single(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (entry == null)
            {
                return RedirectToAction("Schedule", new { userName = username, message = "Course not found. " });
            }

            uow.ClassesTakenRepository.Delete(entry);
            uow.Save();

            return RedirectToAction("Schedule", new { userName = username, message = "Course deleted from schedule. " });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult EditClassTaken(int id, int sid, int cid)
        {
            var model = uow.ClassesTakenRepository.Single(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
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

            uow.ClassesTakenRepository.Update(classTaken);
            uow.Save();

            var member = uow.MemberRepository.SingleById(classTaken.UserId);

            return RedirectToAction("Schedule", new { userName = member.UserName, message = "Record updated." });
        }
    }
}
