namespace DeltaSigmaPhiWebsite.Controllers
{
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class StudyHoursController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var thisSemester = await GetThisSemesterAsync();
            var model = await _db.StudyHours.Where(s => s.DateTimeStudied >= thisSemester.DateStart).ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> Tracker(int? offset)
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
                    Members = await _db.Members.Where(s => s.RequiredStudyHours > 0).ToListAsync(),
                    StartDate = startOfThisWeek,
                    EndDate = endOfWeek
                },
                ThisSemester = await GetThisSemesterAsync()
            };
            return View(model);
        }

        public async Task<ActionResult> Unapproved()
        {
            var currentUser = await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name);
            if((currentUser.RequiredStudyHours > 0 || currentUser.ProctoredStudyHours > 0) && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                var thisSemester = await GetThisSemesterAsync();
                var startOfStudyHours = thisSemester.StudyHourStart.AddDays(-1);
                var model = _db.StudyHours.Where(s =>
                    s.DateTimeStudied >= startOfStudyHours &&
                    s.ApproverId == null &&
                    s.Submitter.UserName == User.Identity.Name).ToList();
                return View(model);
            }
            else
            {
                var thisSemester = await GetThisSemesterAsync();
                var startOfStudyHours = thisSemester.StudyHourStart.AddDays(-1);
                var model = await _db.StudyHours
                    .Where(s => s.DateTimeStudied >= startOfStudyHours && 
                                s.ApproverId == null)
                    .ToListAsync();
                return View(model);
            }
        }

        public async Task<ActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var studyHour = await _db.StudyHours.SingleAsync(s => s.StudyHourId == id);
            if (studyHour == null)
            {
                return HttpNotFound();
            }
            studyHour.DateTimeApproved = DateTime.UtcNow;
            studyHour.ApproverId = WebSecurity.GetUserId(User.Identity.Name);

            _db.Entry(studyHour).State = EntityState.Modified;
            await _db.SaveChangesAsync();

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
        public async Task<ActionResult> Submit(StudyHour model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index");
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            var member = await _db.Members.FindAsync(userId);
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
            var submitter = await _db.Members.FindAsync(model.SubmittedBy);
            model.RequiredStudyHours = submitter.RequiredStudyHours;
            model.ProctoredStudyHours = submitter.ProctoredStudyHours;

            _db.StudyHours.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Sphinx");
        }
        
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var studyHour = await _db.StudyHours.SingleAsync(s => s.StudyHourId == id);
            if (studyHour == null)
            {
                return HttpNotFound();
            }
            _db.StudyHours.Remove(studyHour);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var member = await _db.Members.FindAsync(id);
            if (member == null)
            {
                return HttpNotFound();
            }

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit([Bind(Include = "UserId,RequiredStudyHours")] Member model)
        {
            var member = await _db.Members.FindAsync(model.UserId);
            if (member == null)
            {
                return HttpNotFound();
            }

            member.RequiredStudyHours = model.RequiredStudyHours;
            _db.Entry(member).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}