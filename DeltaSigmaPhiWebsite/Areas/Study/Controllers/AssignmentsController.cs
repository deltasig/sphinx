namespace DeltaSigmaPhiWebsite.Areas.Study.Controllers
{
    using System.Net;
    using System.Net.Mail;
    using App_Start;
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class AssignmentsController : BaseController
    {
        public async Task<ActionResult> Index(StudyHourIndexModel model, AssignmentsController.AssignmentMessageId? message)
        {
            switch (message)
            {
                case AssignmentMessageId.EditSuccess:
                case AssignmentMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            if (model.SelectedSemester == null)
            {
                model.SelectedSemester = await GetThisSemestersIdAsync();
            }

            model.SemesterList = await GetSemesterListAsync();
            var thisSemester = await _db.Semesters.FindAsync(model.SelectedSemester);
            model.StudyHourAssignments = await _db.StudyHourAssignments
                .Where(a =>
                    a.Start >= thisSemester.DateStart &&
                    a.End <= thisSemester.DateEnd)
                .OrderByDescending(o => o.Start)
                .ToListAsync();
            foreach (var a in model.StudyHourAssignments)
            {
                a.Start = base.ConvertUtcToCst(a.Start);
                a.End = base.ConvertUtcToCst(a.End);
            }

            return View(model);
        }
        public async Task<ActionResult> Info(int? id, AssignmentMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            switch (message)
            {
                case AssignmentMessageId.AssignMemberUnspecifiedFailure:
                case AssignmentMessageId.AssignMemberImproperDeletionFailure:
                case AssignmentMessageId.AssignMemberInsertionDuplicateFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case AssignmentMessageId.AssignMemberSuccess:
                case AssignmentMessageId.EditSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var model = new StudyHourAssignmentInfoModel
            {
                Assignment = await _db.StudyHourAssignments.FindAsync(id),
                Members = await base.GetUserIdListAsFullNameAsync()
            };
            model.Assignment.Start = base.ConvertUtcToCst(model.Assignment.Start);
            model.Assignment.End = base.ConvertUtcToCst(model.Assignment.End);

            return View(model);
        }

        public ActionResult Create(AssignmentMessageId? message, string additionalMessageInfo)
        {
            switch (message)
            {
                case AssignmentMessageId.CreateSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message) + additionalMessageInfo;
                    break;
                case AssignmentMessageId.CreateUnspecifiedFailure:
                case AssignmentMessageId.CreateImproperDateFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
            }

            var model = new StudyHourAssignment
            {
                Start = base.GetStartOfCurrentWeek(),
                End = base.GetStartOfCurrentWeek().AddDays(7).AddSeconds(-1)
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StudyHourAssignment model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Create", "Assignments", new
            {
                message = AssignmentMessageId.CreateUnspecifiedFailure
            });

            model.Start = base.ConvertCstToUtc(model.Start);
            model.End = base.ConvertCstToUtc(model.End);

            var semester = await _db.Semesters
                .SingleOrDefaultAsync(s => 
                    model.Start >= s.DateStart && 
                    model.End <= s.DateEnd);

            if(semester == null)
            {
                return RedirectToAction("Create", "Assignments", new
                {
                    message = AssignmentMessageId.CreateImproperDateFailure
                });
            }

            _db.StudyHourAssignments.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Create", "Assignments", new
            {
                message = AssignmentMessageId.CreateSuccess, 
                additionalMessageInfo = semester + "."
            });
        }

        public async Task<ActionResult> Edit(int? id, AssignmentMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            switch (message)
            {
                case AssignmentMessageId.EditImproperDateFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
            }

            var studyHourAssignment = await _db.StudyHourAssignments.FindAsync(id);
            if (studyHourAssignment == null)
            {
                return HttpNotFound();
            }

            studyHourAssignment.Start = base.ConvertUtcToCst(studyHourAssignment.Start);
            studyHourAssignment.End = base.ConvertUtcToCst(studyHourAssignment.End);

            return View(studyHourAssignment);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StudyHourAssignment model)
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
                return RedirectToAction("Edit", "Assignments", new
                {
                    id = model.StudyHourAssignmentId,
                    message = AssignmentMessageId.EditImproperDateFailure
                });
            }

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Assignments", new
            {
                message = AssignmentMessageId.EditSuccess
            });
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studyHourAssignment = await _db.StudyHourAssignments.FindAsync(id);
            if (studyHourAssignment == null)
            {
                return HttpNotFound();
            }

            studyHourAssignment.Start = base.ConvertUtcToCst(studyHourAssignment.Start);
            studyHourAssignment.End = base.ConvertUtcToCst(studyHourAssignment.End);

            return View(studyHourAssignment);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var studyHourAssignment = await _db.StudyHourAssignments.FindAsync(id);
            _db.StudyHourAssignments.Remove(studyHourAssignment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Assignments", new
            {
                message = AssignmentMessageId.DeleteSuccess
            });
        }
        
        public async Task<ActionResult> Assign()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Assign(StudyHourAssignmentInfoModel model)
        {
            if (!ModelState.IsValid && model.SelectedMemberIds == null)
            {
                return RedirectToAction("Info", "Assignments", new
                {
                    id = model.Assignment.StudyHourAssignmentId,
                    message = AssignmentMessageId.AssignMemberUnspecifiedFailure
                });
            }

            var assignment = await _db.StudyHourAssignments.FindAsync(model.Assignment.StudyHourAssignmentId);
            var assignedMemberIds = assignment.MembersAssigned.Select(m => m.AssignedMemberId).ToList();
            
            // Add new member assignments.
            foreach (var id in model.SelectedMemberIds)
            {
                if(assignedMemberIds.Contains(id))
                {
                    return RedirectToAction("Info", "Assignments", new
                    {
                        id = model.Assignment.StudyHourAssignmentId,
                        message = AssignmentMessageId.AssignMemberInsertionDuplicateFailure
                    });
                }

                var memberAssignment = new MemberStudyHourAssignment
                {
                    AssignedMemberId = id,
                    AssignmentId = model.Assignment.StudyHourAssignmentId,
                    UnproctoredAmount = model.MemberAssignment.UnproctoredAmount,
                    ProctoredAmount = model.MemberAssignment.ProctoredAmount,
                    AssignedOn = DateTime.UtcNow
                };
                _db.MemberStudyHourAssignments.Add(memberAssignment);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Info", "Assignments", new
            {
                id = model.Assignment.StudyHourAssignmentId,
                message = AssignmentMessageId.AssignMemberSuccess
            });
        }

        public async Task<ActionResult> EditMemberAssignment(int? id, AssignmentMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = await _db.MemberStudyHourAssignments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            model.Assignment.Start = base.ConvertUtcToCst(model.Assignment.Start);
            model.Assignment.End = base.ConvertUtcToCst(model.Assignment.End);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditMemberAssignment(MemberStudyHourAssignment model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Info", "Assignments", new
                {
                    id = model.AssignmentId,
                    message = AssignmentMessageId.EditUnspecifiedFailure
                });
            }
            var entity = await _db.MemberStudyHourAssignments.FindAsync(model.MemberStudyHourAssignmentId);
            entity.UnproctoredAmount = model.UnproctoredAmount;
            entity.ProctoredAmount = model.ProctoredAmount;
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Info", "Assignments", new
            {
                id = model.AssignmentId,
                message = AssignmentMessageId.EditSuccess
            });
        }

        public async Task<ActionResult> Unassign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.MemberStudyHourAssignments.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            model.Assignment.Start = base.ConvertUtcToCst(model.Assignment.Start);
            model.Assignment.End = base.ConvertUtcToCst(model.Assignment.End);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Unassign")]
        public async Task<ActionResult> UnassignConfirmed(int id)
        {
            var model = await _db.MemberStudyHourAssignments.FindAsync(id);
            _db.MemberStudyHourAssignments.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Info", "Assignments", new
            {
                id = model.AssignmentId,
                message = AssignmentMessageId.DeleteSuccess
            });
        }

        public static dynamic GetResultMessage(AssignmentMessageId? message)
        {
            return message == AssignmentMessageId.CreateUnspecifiedFailure ? "Failed to add assignment, please check your input."
                : message == AssignmentMessageId.CreateImproperDateFailure ? "Failed to add assignment because your specified datetimes do not fall within a defined semester."
                : message == AssignmentMessageId.CreateSuccess ? "Assignment successfully added to "
                : message == AssignmentMessageId.EditImproperDateFailure ? "Failed to modify assignment because your specified datetimes do not fall within a defined semester."
                : message == AssignmentMessageId.EditUnspecifiedFailure ? "Failed to modify assignment for an unknown reason, please contact your administrator."
                : message == AssignmentMessageId.EditSuccess ? "Assignment successfully updated."
                : message == AssignmentMessageId.DeleteSuccess ? "Assignment was successfully deleted."
                : message == AssignmentMessageId.AssignMemberUnspecifiedFailure ? "Assignment failed for an unknown reason, please contact your administrator."
                : message == AssignmentMessageId.AssignMemberImproperDeletionFailure ? "Assignment failed because some members selected for deletion have already submitted study hours to this assignment."
                : message == AssignmentMessageId.AssignMemberInsertionDuplicateFailure ? "Assignment failed because some members selected for assignment have already been assigned."
                : message == AssignmentMessageId.AssignMemberSuccess ? "Assignment was successful."
                : "";
        }

        public enum AssignmentMessageId
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