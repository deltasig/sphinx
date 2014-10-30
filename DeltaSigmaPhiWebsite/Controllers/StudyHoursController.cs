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

        public ActionResult Tracker(int? offset)
        {
            var startOfThisWeek = GetStartOfCurrentWeek();
            var endOfWeek = DateTime.UtcNow;
            var modelOffset = 0;
            if(offset != null && offset < 0)
            {
                startOfThisWeek = startOfThisWeek.AddDays(7 * (int)offset);
                endOfWeek = startOfThisWeek.AddDays(7);
                modelOffset = (int)offset;
            }

            var model = new TrackerModel
            {
                Offset = modelOffset,
                ThisWeek = new ProgressModel
                {
                    Members = uow.MemberRepository.SelectBy(s => s.RequiredStudyHours > 0).ToList(),
                    StartDate = startOfThisWeek,
                    EndDate = endOfWeek
                },
                ThisSemester = GetThisSemester()
            };
            return View(model);
        }

        public ActionResult Unapproved()
        {
            var currentUser = uow.MemberRepository.SelectBy(m => m.UserName == User.Identity.Name).Single();
            if((currentUser.RequiredStudyHours > 0 || currentUser.ProctoredStudyHours > 0) && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                var thisSemester = GetThisSemester();
                var startOfStudyHours = thisSemester.StudyHourStart.AddDays(-1);
                var model = uow.StudyHourRepository.SelectBy(s =>
                    s.DateTimeStudied >= startOfStudyHours &&
                    s.ApproverId == null &&
                    s.Submitter.UserName == User.Identity.Name).ToList();
                return View(model);
            }
            else
            {
                var thisSemester = GetThisSemester();
                var startOfStudyHours = thisSemester.StudyHourStart.AddDays(-1);
                var model = uow.StudyHourRepository.SelectBy(s =>
                    s.DateTimeStudied >= startOfStudyHours &&
                    s.ApproverId == null).ToList();
                return View(model);
            }
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
            studyHour.DateTimeApproved = DateTime.UtcNow;
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
            var userId = WebSecurity.GetUserId(User.Identity.Name);
            //if (uow.StudyHourRepository
            //    .SelectBy(s => s.DateTimeStudied == model.DateTimeStudied && s.SubmittedBy == userId).Any())
            //    return RedirectToAction("Index", "Sphinx", new { message = "You can only submit one study hour entry for a given day." });

            var member = uow.MemberRepository.SingleById(userId);
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var totalHours = member.SubmittedStudyHours.Where(h => h.DateTimeStudied == startOfTodayUtc).Sum(h => h.DurationHours);
            if (model.DurationHours > 6 || model.DurationHours < 0.5 || Math.Abs(model.DurationHours % 0.5) > 0 || totalHours + model.DurationHours > 6)
                return RedirectToAction("Index", "Sphinx", new { message = "You can only submit from 0.5 to 6 hours per day in incremements of 0.5." });
            if (model.DateTimeStudied > ConvertUtcToCst(DateTime.UtcNow))
                return RedirectToAction("Index", "Sphinx", new { message = "You can't submit hours that go into the future." });
            var startOfWeek = GetStartOfCurrentWeek();
            if (model.DateTimeStudied < startOfWeek || model.DateTimeSubmitted > DateTime.UtcNow)
                return RedirectToAction("Index", "Sphinx", new { message = "You can only submit hours for dates occurring after " + startOfWeek.AddDays(-1).ToShortDateString() });

            model.SubmittedBy = userId;
            model.DateTimeStudied = ConvertCstToUtc(model.DateTimeStudied);
            model.DateTimeSubmitted = DateTime.UtcNow;
            var submitter = uow.MemberRepository.SingleById(model.SubmittedBy);
            model.RequiredStudyHours = submitter.RequiredStudyHours;
            model.ProctoredStudyHours = submitter.ProctoredStudyHours;

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