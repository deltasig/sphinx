namespace DeltaSigmaPhiWebsite.Areas.Study.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class AssignmentsController : BaseController
    {
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Assign(StudyPeriodInfoModel model)
        {
            if (!ModelState.IsValid && model.SelectedMemberIds == null)
            {
                return RedirectToAction("Info", "Periods", new
                {
                    id = model.Period.PeriodId,
                    message = PeriodsController.PeriodMessageId.AssignMemberUnspecifiedFailure
                });
            }

            var assignment = await _db.StudyPeriods.FindAsync(model.Period.PeriodId);
            var assignedMemberIds = assignment.Assignments.Select(m => m.AssignedMemberId).ToList();
            
            // Add new member assignments.
            foreach (var id in model.SelectedMemberIds)
            {
                if(assignedMemberIds.Contains(id))
                {
                    return RedirectToAction("Info", "Periods", new
                    {
                        id = model.Period.PeriodId,
                        message = PeriodsController.PeriodMessageId.AssignMemberInsertionDuplicateFailure
                    });
                }

                var memberAssignment = new StudyAssignment
                {
                    AssignedMemberId = id,
                    PeriodId = model.Period.PeriodId,
                    UnproctoredAmount = model.Assignment.UnproctoredAmount,
                    ProctoredAmount = model.Assignment.ProctoredAmount,
                    AssignedOn = DateTime.UtcNow
                };
                _db.StudyAssignments.Add(memberAssignment);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Info", "Periods", new
            {
                id = model.Period.PeriodId,
                message = PeriodsController.PeriodMessageId.AssignMemberSuccess
            });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = await _db.StudyAssignments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            model.Period.Start = base.ConvertUtcToCst(model.Period.Start);
            model.Period.End = base.ConvertUtcToCst(model.Period.End);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(StudyAssignment model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Info", "Periods", new
                {
                    id = model.PeriodId,
                    message = PeriodsController.PeriodMessageId.EditUnspecifiedFailure
                });
            }
            var entity = await _db.StudyAssignments.FindAsync(model.StudyAssignmentId);
            entity.UnproctoredAmount = model.UnproctoredAmount;
            entity.ProctoredAmount = model.ProctoredAmount;
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Info", "Periods", new
            {
                id = model.PeriodId,
                message = PeriodsController.PeriodMessageId.EditSuccess
            });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.StudyAssignments.FindAsync(id);
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Unassign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.StudyAssignments.FindAsync(id);
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            model.Period.Start = base.ConvertUtcToCst(model.Period.Start);
            model.Period.End = base.ConvertUtcToCst(model.Period.End);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Unassign")]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> UnassignConfirmed(int id)
        {
            var model = await _db.StudyAssignments.FindAsync(id);
            _db.StudyAssignments.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Info", "Periods", new
            {
                id = model.PeriodId,
                message = PeriodsController.PeriodMessageId.DeleteSuccess
            });
        }
    }
}