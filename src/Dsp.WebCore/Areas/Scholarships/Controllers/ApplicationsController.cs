namespace Dsp.WebCore.Areas.Scholarships.Controllers;

using Dsp.Data.Entities;
using Dsp.WebCore.Controllers;
using Extensions;
using MarkdownSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public class ApplicationsController : BaseController
{
    [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, New")]
    public async Task<ActionResult> Index()
    {
        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];

        var apps = await Context.ScholarshipApps.ToListAsync();

        foreach (var a in apps)
        {
            a.OpensOn = a.OpensOn.FromUtcToCst();
            a.ClosesOn = a.ClosesOn.FromUtcToCst();
        }

        return View(apps);
    }

    [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, New")]
    public async Task<ActionResult> Details(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var scholarshipApp = await Context.ScholarshipApps.FindAsync(id);
        if (scholarshipApp == null) return NotFound();

        scholarshipApp.OpensOn = scholarshipApp.OpensOn.FromUtcToCst();
        scholarshipApp.ClosesOn = scholarshipApp.ClosesOn.FromUtcToCst();

        foreach (var s in scholarshipApp.Submissions)
        {
            s.SubmittedOn = s.SubmittedOn.FromUtcToCst();
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
        model.Application.OpensOn = model.Application.OpensOn.FromCstToUtc();
        model.Application.ClosesOn = model.Application.ClosesOn.FromCstToUtc();
        Context.ScholarshipApps.Add(model.Application);
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
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Application created successfully";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var scholarshipApp = await Context.ScholarshipApps.FindAsync(id);
        if (scholarshipApp == null) return NotFound();

        var currentQuestions = scholarshipApp.Questions.Select(q => q.Question).ToList();
        var model = new CreateScholarshipAppModel();
        model.Application = scholarshipApp;
        model.Application.OpensOn = model.Application.OpensOn.FromUtcToCst();
        model.Application.ClosesOn = model.Application.ClosesOn.FromUtcToCst();
        model.Questions = new List<QuestionSelectionModel>();
        model.Types = await base.GetScholarshipTypesSelectListAsync();

        var questions = await Context.ScholarshipQuestions.ToListAsync();
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

        model.Application.OpensOn = model.Application.OpensOn.FromCstToUtc();
        model.Application.ClosesOn = model.Application.ClosesOn.FromCstToUtc();
        Context.Entry(model.Application).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        var scholarshipApp = await Context.ScholarshipApps
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
            Context.Entry(s).State = EntityState.Deleted;
        }
        await Context.SaveChangesAsync();

        var selections = model.Questions.Where(q => q.IsSelected);
        foreach (var s in selections)
        {
            scholarshipApp.Questions.Add(s.Question);
        }
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Application updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var scholarshipApp = await Context.ScholarshipApps.FindAsync(id);
        if (scholarshipApp == null) return NotFound();

        return View(scholarshipApp);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var scholarshipApp = await Context.ScholarshipApps.FindAsync(id);
        Context.ScholarshipApps.Remove(scholarshipApp);
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Application deleted successfully.";
        return RedirectToAction("Index");
    }

    [AllowAnonymous]
    public async Task<ActionResult> Submit(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var model = new SubmitScholarshipAppModel();
        model.Submission = new ScholarshipSubmission();
        model.App = await Context.ScholarshipApps.FindAsync(id);

        if (model.App == null) return NotFound();

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
        Context.ScholarshipSubmissions.Add(model.Submission);
        await Context.SaveChangesAsync();
        var submissions = await Context.ScholarshipSubmissions.Include(s => s.Application).ToListAsync();
        var submission = submissions
            .Single(s =>
                s.ScholarshipAppId == model.Submission.ScholarshipAppId &&
                s.FirstName == model.Submission.FirstName &&
                s.LastName == model.Submission.LastName &&
                s.SubmittedOn == model.Submission.SubmittedOn);

        foreach (var a in model.Answers)
        {
            a.ScholarshipSubmissionId = submission.ScholarshipSubmissionId;
            Context.ScholarshipAnswers.Add(a);
        }
        await Context.SaveChangesAsync();

        SendSubmissionConfirmationEmail(submission);
        TempData[SuccessMessageKey] = "Your application was submitted successfully. You should receive a confirmation email shortly.";
        return RedirectToAction("Scholarships", "Home", new { Area = "" });
    }

    [Authorize(Roles = "Administrator, Alumnus, Active, Neophyte, New")]
    public async Task<ActionResult> Submission(Guid? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.ScholarshipSubmissions.FindAsync(id);
        if (model == null) return NotFound();

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
        Context.Entry(model).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Application updated successfully.";
        return RedirectToAction("Submission", new { id = model.ScholarshipSubmissionId });
    }

    private async void SendSubmissionConfirmationEmail(ScholarshipSubmission submission)
    {
        //var subject = "Scholarship Application Submitted with Delta Sigma Phi!";
        //var body = "Dear " + submission.FirstName + ", <br/><br/>" +
        //    "This email has been sent to confirm that we have successfully received your application for the " +
        //    submission.Application.Title + ".  <br />" +
        //    "Please refer to ";
        //body += @"<a href=""https://www.deltasig-de.org/scholarships/" + @""">our scholarship page</a> ";
        //body += "for details on when you should expect to hear back. <br/><br/>" +
        //    "Regards, <br/>" +
        //    "<em>The Gentlemen of Delta Sigma Phi</em>";

        //var emailMessage = new EmailMessage
        //{
        //    Subject = subject,
        //    Body = body,
        //    Destination = submission.Email
        //};

        //var emailService = new EmailService();
        //await emailService.SendAsync(emailMessage);
    }
}
