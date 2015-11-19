namespace Dsp.Areas.Scholarships.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Extensions;
    using MarkdownSharp;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class ApplicationsController : BaseController
    {
        [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, Pledge")]
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            var apps = await _db.ScholarshipApps.ToListAsync();

            foreach (var a in apps)
            {
                a.OpensOn = base.ConvertUtcToCst(a.OpensOn);
                a.ClosesOn = base.ConvertUtcToCst(a.ClosesOn);
            }

            return View(apps);
        }

        [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, Pledge")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var scholarshipApp = await _db.ScholarshipApps.FindAsync(id);
            if (scholarshipApp == null) return HttpNotFound();

            scholarshipApp.OpensOn = base.ConvertUtcToCst(scholarshipApp.OpensOn);
            scholarshipApp.ClosesOn = base.ConvertUtcToCst(scholarshipApp.ClosesOn);

            foreach (var s in scholarshipApp.Submissions)
            {
                s.SubmittedOn = base.ConvertUtcToCst(s.SubmittedOn);
            }

            return View(scholarshipApp);
        }

        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> Create()
        {
            var model = new CreateScholarshipAppModel();
            model.Application = new ScholarshipApp();
            model.Types = await base.GetScholarshipTypesSelectListAsync();
            model.Questions = await base.GetScholarshipQuestionsAsync();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> Create(CreateScholarshipAppModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Types = await base.GetScholarshipTypesSelectListAsync();
                model.Questions = await base.GetScholarshipQuestionsAsync();
                return View(model);
            }

            model.Application.Questions = new List<ScholarshipAppQuestion>();
            model.Application.OpensOn = base.ConvertCstToUtc(model.Application.OpensOn);
            model.Application.ClosesOn = base.ConvertCstToUtc(model.Application.ClosesOn);
            _db.ScholarshipApps.Add(model.Application);
            if (model.Questions == null || !model.Questions.Any())
            {
                TempData[FailureMessageKey] = 
                    "Application creation failed because of an internal issue with questions. Please contact your administrator.";
                return RedirectToAction("Index");
            }
            var selections = model.Questions.Where(q => q.IsSelected);
            foreach (var s in selections)
            {
                model.Application.Questions.Add(s.Question);
            }
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Application created successfully";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var scholarshipApp = await _db.ScholarshipApps.FindAsync(id);
            if (scholarshipApp == null) return HttpNotFound();

            var currentQuestions = scholarshipApp.Questions.Select(q => q.Question).ToList();
            var model = new CreateScholarshipAppModel();
            model.Application = scholarshipApp;
            model.Application.OpensOn = base.ConvertUtcToCst(model.Application.OpensOn);
            model.Application.ClosesOn = base.ConvertUtcToCst(model.Application.ClosesOn);
            model.Questions = new List<QuestionSelectionModel>();
            model.Types = await base.GetScholarshipTypesSelectListAsync();

            var questions = await _db.ScholarshipQuestions.ToListAsync();
            foreach (var q in questions)
            {
                var selection = new QuestionSelectionModel();
                if (currentQuestions.Contains(q))
                {
                    selection.Question = scholarshipApp.Questions
                        .Single(s => s.ScholarshipQuestionId == q.ScholarshipQuestionId);
                    selection.IsSelected = true;
                }
                else
                {
                    var appQuestion = new ScholarshipAppQuestion();
                    appQuestion.ScholarshipQuestionId = q.ScholarshipQuestionId;
                    appQuestion.FormOrder = 0;
                    appQuestion.Question = q;

                    selection.Question = appQuestion;
                }
                model.Questions.Add(selection);
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> Edit(CreateScholarshipAppModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Types = await base.GetScholarshipTypesSelectListAsync();
                model.Questions = await base.GetScholarshipQuestionsAsync();
                return View(model);
            }

            model.Application.OpensOn = base.ConvertCstToUtc(model.Application.OpensOn);
            model.Application.ClosesOn = base.ConvertCstToUtc(model.Application.ClosesOn);
            _db.Entry(model.Application).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var scholarshipApp = await _db.ScholarshipApps
                .Include(s => s.Submissions)
                .Include(s => s.Questions)
                .SingleAsync(s => model.Application.ScholarshipAppId == s.ScholarshipAppId);

            if (scholarshipApp.Submissions.Any())
            {
                TempData[SuccessMessageKey] =
                    "Application information was updated but any changes to the question selection were not because submissions have already been made.";
                return RedirectToAction("Index");
            }

            foreach (var s in scholarshipApp.Questions.ToList())
            {
                _db.Entry(s).State = EntityState.Deleted;
            }
            await _db.SaveChangesAsync();

            var selections = model.Questions.Where(q => q.IsSelected);
            foreach (var s in selections)
            {
                scholarshipApp.Questions.Add(s.Question);
            }
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Application updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var scholarshipApp = await _db.ScholarshipApps.FindAsync(id);
            if (scholarshipApp == null) return HttpNotFound();

            return View(scholarshipApp);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var scholarshipApp = await _db.ScholarshipApps.FindAsync(id);
            _db.ScholarshipApps.Remove(scholarshipApp);
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Application deleted successfully.";
            return RedirectToAction("Index");
        }
        
        [AllowAnonymous]
        public async Task<ActionResult> Submit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        
            var model = new SubmitScholarshipAppModel();
            model.Submission = new ScholarshipSubmission();
            model.App = await _db.ScholarshipApps.FindAsync(id);

            if (model.App == null) return HttpNotFound();

            model.Submission.ScholarshipAppId = (int)id;
            model.Answers = new List<ScholarshipAnswer>();
            foreach (var q in model.App.Questions.OrderBy(q => q.FormOrder))
            {
                var answer = new ScholarshipAnswer();
                answer.ScholarshipQuestionId = q.ScholarshipQuestionId;
                answer.Question = q.Question;
                model.Answers.Add(answer);
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(SubmitScholarshipAppModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = 
                    "Your application could not be submitted.  Please check the information you provided and try again.  " +
                    "If you continue to receive this error, please contact the person in charge of the scholarship.";
                return View(model);
            }

            model.Submission.SubmittedOn = DateTime.UtcNow;
            _db.ScholarshipSubmissions.Add(model.Submission);
            await _db.SaveChangesAsync();
            var submissions = await _db.ScholarshipSubmissions.Include(s => s.Application).ToListAsync();
            var submission = submissions
                .Single(s =>
                    s.ScholarshipAppId == model.Submission.ScholarshipAppId &&
                    s.FirstName == model.Submission.FirstName &&
                    s.LastName == model.Submission.LastName &&
                    s.SubmittedOn == model.Submission.SubmittedOn);

            foreach (var a in model.Answers)
            {
                a.ScholarshipSubmissionId = submission.ScholarshipSubmissionId;
                _db.ScholarshipAnswers.Add(a);
            }
            await _db.SaveChangesAsync();

            SendStudyHourSubmissionEmail(submission);
            TempData[SuccessMessageKey] = "Your application was submitted successfully. You should receive a confirmation email shortly.";
            return RedirectToAction("Scholarships", "Home", new { Area = "" });
        }

        [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, Pledge")]
        public async Task<ActionResult> Submission(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.ScholarshipSubmissions.FindAsync(id);
            if (model == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            var markdown = new Markdown();
            ViewBag.CommitteeResponse = markdown.Transform(model.CommitteeResponse);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
        public async Task<ActionResult> EditSubmission(ScholarshipSubmission model)
        {
            if (!ModelState.IsValid)
            {
                TempData[FailureMessageKey] = "Application update failed for an unspecified reason, please contact your administrator.";
                return RedirectToAction("Submission", new { id = model.ScholarshipSubmissionId });
            }

            model.CommitteeRespondedOn = DateTime.UtcNow;
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Application updated successfully.";
            return RedirectToAction("Submission", new { id = model.ScholarshipSubmissionId });
        }

        private async void SendStudyHourSubmissionEmail(ScholarshipSubmission submission)
        {
            var subject = "Scholarship Application Submitted with Delta Sigma Phi!";
            var body = "Dear " + submission.FirstName + ", <br/><br/>" +
                "This email has been sent to confirm that we have successfully received your application for the " + 
                submission.Application.Title + ".  <br />" +
                "Please refer to ";
            body += @"<a href=""https://www.deltasig-de.org/scholarships/" + @""">our scholarship page</a> ";
            body += "for details on when you should expect to hear back. <br/><br/>" +
                "Regards, <br/>" +
                "<em>The Gentlemen of Delta Sigma Phi</em>";

            var emailMessage = new IdentityMessage
            {
                Subject = subject,
                Body = body,
                Destination = submission.Email
            };

            var emailService = new EmailService();
            await emailService.SendAsync(emailMessage);
        }
    }
}
