namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Linq;
    using System.Net;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class StudyHoursController : BaseController
    {
        public StudyHoursController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }
        
        public ActionResult Index()
        {
            var thisSemester = GetThisSemester();
            var model = uow.StudyHourRepository.SelectBy(s => s.DateTimeStudied >= thisSemester.DateStart).ToList();
            return View(model);
        }

        public ActionResult Tracker()
        {
            var thisSemester = GetThisSemester();
            var startOfThisWeek = GetStartOfCurrentWeek();

            var model = new TrackerModel
            {
                ThisWeek = uow.StudyHourRepository.SelectBy(s => s.DateTimeStudied >= startOfThisWeek).ToList(),
                ThisSemester = uow.StudyHourRepository.SelectBy(s => s.DateTimeStudied >= thisSemester.DateStart).ToList()
            };
            return View(model);
        }

        public ActionResult Unapproved()
        {
            var startOfThisWeek = GetStartOfCurrentWeek();
            var model = uow.StudyHourRepository.SelectBy(s =>
                s.DateTimeStudied >= startOfThisWeek && 
                s.ApproverId == null).ToList();
            return View(model);
        }

        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var studyHour = uow.StudyHourRepository.Single(s => s.StudyHourId == id);
            if (studyHour == null)
            {
                return HttpNotFound();
            }
            studyHour.DateTimeApproved = DateTime.Now;
            studyHour.ApproverId = WebSecurity.GetUserId(User.Identity.Name);

            uow.StudyHourRepository.Update(studyHour);
            uow.Save();

            return RedirectToAction("Unapproved");
        }
        
        [HttpGet]
        public ActionResult Submit()
        {
            var model = new StudyHour();
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(StudyHour model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            if (uow.StudyHourRepository.SelectBy(s => s.DateTimeStudied == model.DateTimeStudied).Any())
                return RedirectToAction("Index", "Sphinx", new { message = "You can only submit one study hour entry for a given day."});

            model.SubmittedBy = WebSecurity.GetUserId(User.Identity.Name);
            model.DateTimeStudied = model.DateTimeStudied;
            model.DateTimeSubmitted = DateTime.Now;

            uow.StudyHourRepository.Insert(model);
            uow.Save();

            return RedirectToAction("Index", "Sphinx");
        }
        
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var studyHour = uow.StudyHourRepository.Single(s => s.StudyHourId == id);
            if (studyHour == null)
            {
                return HttpNotFound();
            }
            uow.StudyHourRepository.Delete(studyHour);
            uow.Save();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var member = uow.MemberRepository.SingleById(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult Edit([Bind(Include = "UserId,RequiredStudyHours")] Member model)
        {
            var member = uow.MemberRepository.SingleById(model.UserId);
            if (member == null)
            {
                return HttpNotFound();
            }

            member.RequiredStudyHours = model.RequiredStudyHours;
            uow.MemberRepository.Update(member);
            uow.Save();

            return RedirectToAction("Index");
        }
    }
}