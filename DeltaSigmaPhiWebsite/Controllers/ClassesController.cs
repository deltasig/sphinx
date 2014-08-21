namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Collections.Generic;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using Models.ViewModels;

    
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
            var classes = uow.ClassesRepository.SelectAll();
            return View(classes.ToList());
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Create()
        {
            ViewBag.Message = string.Empty;
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName");
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
                uow.ClassesRepository.Insert(@class);
                uow.Save();
                ViewBag.Message = "Class created successfully. ";
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
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
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
            Class @class = uow.ClassesRepository.SingleById(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult DeleteConfirmed(int id)
        {
            uow.ClassesRepository.DeleteById(id);
            uow.Save();
            return RedirectToAction("Index", new { message = "Course deleted." });
        }

        public ActionResult CreateSchedule()
        {
            var model = new ClassScheduleModel
            {
                Members = GetUserIdListAsFullName(),
                AllClasses = uow.ClassesRepository.SelectAll(),
                Semesters = GetSemesterList(),
                SelectedSemester = GetThisOrLastSemester().SemesterId,
                ClassesTaken = new List<ClassTaken> { new ClassTaken() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult CreateSchedule(ClassScheduleModel model)
        {
            if(ModelState.IsValid)
            {
                foreach(var course in model.ClassesTaken)
                {
                    course.SemesterId = model.SelectedSemester;
                    course.UserId = model.SelectedMember;
                    
                    uow.ClassesTakenRepository.Insert(course);
                }
                uow.Save();
            }
            var member = uow.MemberRepository.SingleById(model.SelectedMember);
            return RedirectToAction("Schedule", new { userName = member.UserName, message = "Class(es) added successfully. " });
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
        public ActionResult Edit(int id, int sid, int cid)
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
        public ActionResult Edit([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName,CreditHours")] ClassTaken classTaken)
        {
            if (!ModelState.IsValid) 
                return RedirectToAction("Schedule", new {userName = classTaken.Member.UserName, message = "Failed to update record."});

            uow.ClassesTakenRepository.Update(classTaken);
            uow.Save();

            return RedirectToAction("Schedule", new { userName = "", message = "Record updated." });
        }
    }
}
