namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Net;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class StudyHoursController : BaseController
    {
        public async Task<ActionResult> Index(StudyHourIndexModel model, AssignmentMessageId? message)
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
            foreach(var a in model.StudyHourAssignments)
            {
                a.Start = base.ConvertUtcToCst(a.Start);
                a.End = base.ConvertUtcToCst(a.End);
            }

            return View(model);
        }

        public ActionResult CreateAssignment(AssignmentMessageId? message, string additionalMessageInfo)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAssignment(StudyHourAssignment model)
        {
            if (!ModelState.IsValid) return RedirectToAction("CreateAssignment", "StudyHours", new
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
                return RedirectToAction("CreateAssignment", "StudyHours", new
                {
                    message = AssignmentMessageId.CreateImproperDateFailure
                });
            }

            _db.StudyHourAssignments.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("CreateAssignment", "StudyHours", new
            {
                message = AssignmentMessageId.CreateSuccess, 
                additionalMessageInfo = semester + "."
            });
        }

        public async Task<ActionResult> EditAssignment(int? id, AssignmentMessageId? message)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAssignment(StudyHourAssignment model)
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
                return RedirectToAction("EditAssignment", "StudyHours", new
                {
                    id = model.StudyHourAssignmentId,
                    message = AssignmentMessageId.EditImproperDateFailure
                });
            }

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "StudyHours", new
            {
                message = AssignmentMessageId.EditSuccess
            });
        }

        public async Task<ActionResult> DeleteAssignment(int? id)
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

        [HttpPost, ActionName("DeleteAssignment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAssignmentConfirmed(int id)
        {
            var studyHourAssignment = await _db.StudyHourAssignments.FindAsync(id);
            _db.StudyHourAssignments.Remove(studyHourAssignment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "StudyHours", new
            {
                message = AssignmentMessageId.DeleteSuccess
            });
        }
        
        public async Task<ActionResult> AssignmentInfo(int? id, AssignmentMessageId? message)
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Assign(StudyHourAssignmentInfoModel model)
        {
            if (!ModelState.IsValid && model.SelectedMemberIds == null)
            {
                return RedirectToAction("AssignmentInfo", "StudyHours", new
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
                    return RedirectToAction("AssignmentInfo", "StudyHours", new
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

            return RedirectToAction("AssignmentInfo", "StudyHours", new
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditMemberAssignment(MemberStudyHourAssignment model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AssignmentInfo", "StudyHours", new
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
            return RedirectToAction("AssignmentInfo", "StudyHours", new
            {
                id = model.AssignmentId,
                message = AssignmentMessageId.EditSuccess
            });
        }

        public async Task<ActionResult> DeleteMemberAssignment(int? id)
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

        [HttpPost, ActionName("DeleteMemberAssignment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMemberAssignmentConfirmed(int id)
        {
            var model = await _db.MemberStudyHourAssignments.FindAsync(id);
            _db.MemberStudyHourAssignments.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("AssignmentInfo", "StudyHours", new
            {
                id = model.AssignmentId,
                message = AssignmentMessageId.DeleteSuccess
            });
        }
        
        public async Task<ActionResult> Unapproved()
        {
            var thisSemester = await base.GetThisSemesterAsync();

            // Check if current user has any study hours this semester.
            var memberAssignments = (await base.GetStudyHourAssignmentsForUserAsync(WebSecurity.CurrentUserId, thisSemester)).ToList();
            if (memberAssignments.Any() || !User.IsInRole("Administrator") || !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new List<StudyHour>();
            var allMemberAssignments = await _db.MemberStudyHourAssignments.ToListAsync();
            foreach (var m in allMemberAssignments)
            {
                model.AddRange(m.TurnIns.Where(s => s.DateTimeApproved == null));
            }
            return View(model.OrderByDescending(o => o.DateTimeStudied));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public async Task<ActionResult> Submit(int? id, AssignmentMessageId? message)
        {

            var memberAssignment = await _db.MemberStudyHourAssignments.FindAsync(id);
            if (memberAssignment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var member = memberAssignment.AssignedMember;
            if (member.UserId != WebSecurity.CurrentUserId && !User.IsInRole("Academics") && !User.IsInRole("Administrator"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            switch (message)
            {
                case AssignmentMessageId.SubmitUnspecifiedFailure:
                case AssignmentMessageId.SubmitOutsideDateRangeFailure:
                case AssignmentMessageId.SubmitIncrementFailure:
                case AssignmentMessageId.SubmitOverLimitForDayFailure:
                case AssignmentMessageId.SubmitDurationRangeFailure:
                case AssignmentMessageId.SubmitFutureFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case AssignmentMessageId.SubmitSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var thisSemester = await base.GetThisSemesterAsync();

            var model = new StudyHourSubmissionModel
            {
                Submission = new StudyHour
                {
                    DateTimeStudied = base.ConvertUtcToCst(DateTime.UtcNow),
                },
                Approvers = await base.GetAllApproverIdsAsync(member.UserId, thisSemester),
                MemberAssignments = 
                    await base.GetStudyHourAssignmentsSelectListForUserAsync(member.UserId, thisSemester)
            };

            if(id != null)
            {
                model.SelectedMemberAssignmentId = memberAssignment.MemberStudyHourAssignmentId;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(StudyHourSubmissionModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitUnspecifiedFailure
                });
            }

            model.Submission.DateTimeStudied = base.ConvertCstToUtc(model.Submission.DateTimeStudied);
            #region Submission Validation
            var memberAssignment = await _db.MemberStudyHourAssignments.FindAsync(model.SelectedMemberAssignmentId);
            if (model.Submission.DateTimeStudied < memberAssignment.Assignment.Start || model.Submission.DateTimeStudied > memberAssignment.Assignment.End)
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitOutsideDateRangeFailure
                });
            }
            if (model.Submission.DurationHours > 6 || model.Submission.DurationHours < 0.5)
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitDurationRangeFailure
                });
            }
            if(Math.Abs(model.Submission.DurationHours % 0.5) > 0 )
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitIncrementFailure
                });
            }
            var totalHours = memberAssignment.TurnIns.Where(h => h.DateTimeStudied.Date == model.Submission.DateTimeStudied.Date).Sum(h => h.DurationHours);
            if(totalHours + model.Submission.DurationHours > 6)
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitOverLimitForDayFailure
                });
            }
            if (model.Submission.DateTimeStudied > DateTime.UtcNow)
            {
                return RedirectToAction("Submit", "StudyHours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = AssignmentMessageId.SubmitFutureFailure
                });
            }
            #endregion
            model.Submission.DateTimeSubmitted = DateTime.UtcNow;
            _db.StudyHours.Add(model.Submission);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Sphinx", new
            {
                message = "Study Hours submitted successfully."
            });
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
        public async Task<ActionResult> Edit(Member model)
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

        private static dynamic GetResultMessage(AssignmentMessageId? message)
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
                : message == AssignmentMessageId.SubmitUnspecifiedFailure ? "hour submission failed for an unknown reason, please contact your administrator"
                : message == AssignmentMessageId.SubmitOutsideDateRangeFailure ? "Study hour submission failed - the date on which you studied did not fall between the dates of the assignment you were given."
                : message == AssignmentMessageId.SubmitIncrementFailure ? "Study hour submission failed - the duration must be in increments of 0.5."
                : message == AssignmentMessageId.SubmitOverLimitForDayFailure ? "Study hour submission failed - you can only submit up to 6 hours in one day."
                : message == AssignmentMessageId.SubmitDurationRangeFailure ? "Study hour submission failed - keep the duration between 0.5 and 6 hours."
                : message == AssignmentMessageId.SubmitFutureFailure ? "Study hour submission failed - you can't say that you studied in the future."
                : message == AssignmentMessageId.SubmitSuccess ? "Study hours were submitted successfully."
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
            AssignMemberSuccess,
            SubmitUnspecifiedFailure,
            SubmitOutsideDateRangeFailure,
            SubmitIncrementFailure,
            SubmitOverLimitForDayFailure,
            SubmitDurationRangeFailure,
            SubmitFutureFailure,
            SubmitSuccess,
        }
    }
}