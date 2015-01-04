namespace DeltaSigmaPhiWebsite.Areas.Study.Controllers
{
    using App_Start;
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class HoursController : BaseController
    {
        public async Task<ActionResult> Unapproved()
        {
            var thisSemester = await base.GetThisSemesterAsync();

            // Check if current user has any study hours this semester.
            var memberAssignments = (await base.GetStudyHourAssignmentsForUserAsync(WebSecurity.CurrentUserId, thisSemester)).ToList();
            if (memberAssignments.Any())
            {
                if (!User.IsInRole("Administrator") && !User.IsInRole("Academics"))
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

        [HttpGet]
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

            return RedirectToAction("Index", "Home", new
            {
                area = "Sphinx", 
                message = "You successfully approved " + studyHour.MemberAssignment.AssignedMember + "'s study hours!"
            });
        }

        public async Task<ActionResult> Submit(int? id, HoursMessageId? message)
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
                case HoursMessageId.SubmitUnspecifiedFailure:
                case HoursMessageId.SubmitOutsideDateRangeFailure:
                case HoursMessageId.SubmitIncrementFailure:
                case HoursMessageId.SubmitOverLimitForDayFailure:
                case HoursMessageId.SubmitDurationRangeFailure:
                case HoursMessageId.SubmitFutureFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case HoursMessageId.SubmitSuccess:
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
                model.SelectedMemberAssignmentId = (int)id;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(StudyHourSubmissionModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitUnspecifiedFailure
                });
            }

            model.Submission.DateTimeStudied = base.ConvertCstToUtc(model.Submission.DateTimeStudied);
            #region Submission Validation
            var memberAssignment = await _db.MemberStudyHourAssignments.FindAsync(model.Submission.AssignmentId);
            if (model.Submission.DateTimeStudied < memberAssignment.Assignment.Start || model.Submission.DateTimeStudied > memberAssignment.Assignment.End)
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitOutsideDateRangeFailure
                });
            }
            if (model.Submission.DurationHours > 6 || model.Submission.DurationHours < 0.5)
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitDurationRangeFailure
                });
            }
            if(Math.Abs(model.Submission.DurationHours % 0.5) > 0 )
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitIncrementFailure
                });
            }
            var totalHours = memberAssignment.TurnIns.Where(h => h.DateTimeStudied.Date == model.Submission.DateTimeStudied.Date).Sum(h => h.DurationHours);
            if(totalHours + model.Submission.DurationHours > 6)
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitOverLimitForDayFailure
                });
            }
            if (model.Submission.DateTimeStudied > DateTime.UtcNow)
            {
                return RedirectToAction("Submit", "Hours", new
                {
                    id = model.SelectedMemberAssignmentId,
                    message = HoursMessageId.SubmitFutureFailure
                });
            }
            #endregion
            model.Submission.DateTimeSubmitted = DateTime.UtcNow;
            _db.StudyHours.Add(model.Submission);
            await _db.SaveChangesAsync();

            var submission = await _db.StudyHours
                .SingleAsync(s => 
                    s.AssignmentId == model.Submission.AssignmentId && 
                    s.DateTimeStudied == model.Submission.DateTimeStudied &&
                    s.ApproverId == model.Submission.ApproverId);

            SendStudyHourSubmissionEmail(submission);

            return RedirectToAction("Index", "Home", new
            {
                message = "Study Hours submitted successfully.",
                area = "Sphinx"
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

        public static dynamic GetResultMessage(HoursMessageId? message)
        {
            return message == HoursMessageId.SubmitUnspecifiedFailure ? "hour submission failed for an unknown reason, please contact your administrator"
                : message == HoursMessageId.SubmitOutsideDateRangeFailure ? "Study hour submission failed - the date on which you studied did not fall between the dates of the assignment you were given."
                : message == HoursMessageId.SubmitIncrementFailure ? "Study hour submission failed - the duration must be in increments of 0.5."
                : message == HoursMessageId.SubmitOverLimitForDayFailure ? "Study hour submission failed - you can only submit up to 6 hours in one day."
                : message == HoursMessageId.SubmitDurationRangeFailure ? "Study hour submission failed - keep the duration between 0.5 and 6 hours."
                : message == HoursMessageId.SubmitFutureFailure ? "Study hour submission failed - you can't say that you studied in the future."
                : message == HoursMessageId.SubmitSuccess ? "Study hours were submitted successfully."
                : "";
        }

        public enum HoursMessageId
        {
            SubmitUnspecifiedFailure,
            SubmitOutsideDateRangeFailure,
            SubmitIncrementFailure,
            SubmitOverLimitForDayFailure,
            SubmitDurationRangeFailure,
            SubmitFutureFailure,
            SubmitSuccess,
        }
        private async void SendStudyHourSubmissionEmail(StudyHour submission)
        {
            var subject = "(Sphinx) Study Hour Approval Request: " + submission.MemberAssignment.AssignedMember + ", " + submission.DateTimeStudied;
            var body = submission.MemberAssignment.AssignedMember + " has requested that you approve his study hours (" + 
                submission.DurationHours + " hours).  " + "To approve them, ";
            body += @"<a href=""https://www.deltasig-de.org/study/hours/approve/" + submission.StudyHourId + @""">click here</a>.";

            var emailMessage = new IdentityMessage
            {
                Subject = subject,
                Body = body,
                Destination = submission.Approver.Email
            };

            var emailService = new EmailService();
            await emailService.SendAsync(emailMessage);
        }
    }
}