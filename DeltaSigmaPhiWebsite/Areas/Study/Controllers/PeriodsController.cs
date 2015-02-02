namespace DeltaSigmaPhiWebsite.Areas.Study.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class PeriodsController : BaseController
    {
        public async Task<ActionResult> Index(StudyPeriodIndexModel model, PeriodMessageId? message)
        {
            switch (message)
            {
                case PeriodMessageId.EditSuccess:
                case PeriodMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            if (model.SelectedSemester == null)
            {
                model.SelectedSemester = await GetThisSemestersIdAsync();
            }

            model.SemesterList = await GetSemesterListAsync();
            var thisSemester = await _db.Semesters.FindAsync(model.SelectedSemester);
            model.Periods = await _db.StudyPeriods
                .Where(a =>
                    a.Start >= thisSemester.DateStart &&
                    a.End <= thisSemester.DateEnd)
                .OrderByDescending(o => o.Start)
                .ToListAsync();
            foreach (var a in model.Periods)
            {
                a.Start = base.ConvertUtcToCst(a.Start);
                a.End = base.ConvertUtcToCst(a.End);
            }

            return View(model);
        }

        [Authorize(Roles = "Academics, Sergeant-At-Arms, President, Secretary, Administrator")]
        public async Task<ActionResult> Info(int? id, PeriodMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            switch (message)
            {
                case PeriodMessageId.AssignMemberUnspecifiedFailure:
                case PeriodMessageId.AssignMemberImproperDeletionFailure:
                case PeriodMessageId.AssignMemberInsertionDuplicateFailure:
                case PeriodMessageId.EditUnspecifiedFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case PeriodMessageId.AssignMemberSuccess:
                case PeriodMessageId.EditSuccess:
                case PeriodMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var model = new StudyPeriodInfoModel
            {
                Period = await _db.StudyPeriods.FindAsync(id),
                Members = await base.GetUserIdListAsFullNameAsync()
            };
            model.Period.Start = base.ConvertUtcToCst(model.Period.Start);
            model.Period.End = base.ConvertUtcToCst(model.Period.End);

            return View(model);
        }

        [Authorize(Roles = "Academics, Administrator")]
        public ActionResult Create(PeriodMessageId? message, string additionalMessageInfo)
        {
            switch (message)
            {
                case PeriodMessageId.CreateSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message) + additionalMessageInfo;
                    break;
                case PeriodMessageId.CreateUnspecifiedFailure:
                case PeriodMessageId.CreateImproperDateFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
            }

            var model = new StudyPeriod
            {
                Start = base.GetStartOfCurrentStudyWeek(),
                End = base.GetStartOfCurrentStudyWeek().AddDays(7).AddSeconds(-1)
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Academics, Administrator")]
        public async Task<ActionResult> Create(StudyPeriod model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Create", "Periods", new
            {
                message = PeriodMessageId.CreateUnspecifiedFailure
            });

            model.Start = base.ConvertCstToUtc(model.Start);
            model.End = base.ConvertCstToUtc(model.End);

            var semester = await _db.Semesters
                .SingleOrDefaultAsync(s => 
                    model.Start >= s.DateStart && 
                    model.End <= s.DateEnd);

            if(semester == null)
            {
                return RedirectToAction("Create", "Periods", new
                {
                    message = PeriodMessageId.CreateImproperDateFailure
                });
            }

            _db.StudyPeriods.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Create", "Periods", new
            {
                message = PeriodMessageId.CreateSuccess, 
                additionalMessageInfo = semester + "."
            });
        }

        [Authorize(Roles = "Academics, Administrator")]
        public async Task<ActionResult> Edit(int? id, PeriodMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            switch (message)
            {
                case PeriodMessageId.EditImproperDateFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
            }

            var period = await _db.StudyPeriods.FindAsync(id);
            if (period == null)
            {
                return HttpNotFound();
            }

            period.Start = base.ConvertUtcToCst(period.Start);
            period.End = base.ConvertUtcToCst(period.End);

            return View(period);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Academics, Administrator")]
        public async Task<ActionResult> Edit(StudyPeriod model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Start = base.ConvertCstToUtc(model.Start);
            model.End = base.ConvertCstToUtc(model.End);

            var semester = await _db.Semesters
                .SingleOrDefaultAsync(s =>
                    model.Start >= s.DateStart &&
                    model.End <= s.DateEnd);

            if (semester == null)
            {
                return RedirectToAction("Edit", "Periods", new
                {
                    id = model.PeriodId,
                    message = PeriodMessageId.EditImproperDateFailure
                });
            }

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Periods", new
            {
                message = PeriodMessageId.EditSuccess
            });
        }

        [Authorize(Roles = "Academics, Administrator")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var period = await _db.StudyPeriods.FindAsync(id);
            if (period == null)
            {
                return HttpNotFound();
            }

            period.Start = base.ConvertUtcToCst(period.Start);
            period.End = base.ConvertUtcToCst(period.End);

            return View(period);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        [Authorize(Roles = "Academics, Administrator")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var period = await _db.StudyPeriods.FindAsync(id);
            _db.StudyPeriods.Remove(period);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Periods", new
            {
                message = PeriodMessageId.DeleteSuccess
            });
        }

        public static dynamic GetResultMessage(PeriodMessageId? message)
        {
            return message == PeriodMessageId.CreateUnspecifiedFailure ? "Failed to add assignment, please check your input."
                : message == PeriodMessageId.CreateImproperDateFailure ? "Failed to add assignment because your specified datetimes do not fall within a defined semester."
                : message == PeriodMessageId.CreateSuccess ? "Assignment successfully added to "
                : message == PeriodMessageId.EditImproperDateFailure ? "Failed to modify assignment because your specified datetimes do not fall within a defined semester."
                : message == PeriodMessageId.EditUnspecifiedFailure ? "Failed to modify assignment for an unknown reason, please contact your administrator."
                : message == PeriodMessageId.EditSuccess ? "Assignment successfully updated."
                : message == PeriodMessageId.DeleteSuccess ? "Assignment was successfully deleted."
                : message == PeriodMessageId.AssignMemberUnspecifiedFailure ? "Assignment failed for an unknown reason, please contact your administrator."
                : message == PeriodMessageId.AssignMemberImproperDeletionFailure ? "Assignment failed because some members selected for deletion have already submitted study hours to this assignment."
                : message == PeriodMessageId.AssignMemberInsertionDuplicateFailure ? "Assignment failed because some members selected for assignment have already been assigned."
                : message == PeriodMessageId.AssignMemberSuccess ? "Assignment was successful."
                : "";
        }

        public enum PeriodMessageId
        {
            CreateUnspecifiedFailure,
            CreateImproperDateFailure,
            CreateSuccess,
            EditSuccess,
            EditImproperDateFailure,
            EditUnspecifiedFailure,
            DeleteSuccess,
            AssignMemberUnspecifiedFailure,
            AssignMemberImproperDeletionFailure,
            AssignMemberInsertionDuplicateFailure,
            AssignMemberSuccess
        }
    }
}