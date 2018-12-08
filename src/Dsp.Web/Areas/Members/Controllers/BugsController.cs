using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Repositories;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Dsp.Web.Areas.Members.Models;
using Dsp.Web.Controllers;
using Dsp.Web.Extensions;
using Microsoft.AspNet.Identity;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Dsp.Web.Areas.Members.Controllers
{
    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate, Advisor")]
    public class BugsController : BaseController
    {
        private const string ADMIN_USERNAME = "tjm6f4";
        private const string ADMIN_EMAIL = "tjm6f4@mst.edu";
        private IBugService _bugService;

        public BugsController()
        {
            _bugService = new BugService(new Repository<SphinxDbContext>(_db));
        }

        [HttpGet]
        public async Task<ActionResult> Index(BugsIndexFilterModel filter)
        {
            var serviceResult = await _bugService.GetBugReportsAsync(
                filter.page,
                filter.pageSize,
                filter.includeFixed,
                filter.search
            );
            var filterResults = serviceResult.Item1;
            var totalPages = serviceResult.Item2;
            var openCount = serviceResult.Item3;
            var fixedCount = serviceResult.Item4;

            var model = new BugsIndexModel(filterResults, filter, totalPages, openCount, fixedCount);

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _bugService.GetBugReportByIdAsync(id);

            if (model == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        public ActionResult Submit(string urlWithProblem)
        {
            var model = new BugReport();
            model.UrlWithProblem = urlWithProblem;
            model.UserId = User.Identity.GetUserId<int>();
            model.ReportedOn = DateTime.UtcNow.FromUtcToCst();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(BugReport model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Something went wrong while submitting the bug report.";
                return View(model);
            }

            model.UserId = User.Identity.GetUserId<int>();
            await _bugService.CreateBugReportAsync(model);

            // Send email to admin
            var body = RenderRazorViewToString("~/Views/Emails/NewBugReport.cshtml", model);
            var bytes = Encoding.Default.GetBytes(body);
            body = Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = "[SPHINX BUG REPORT] - " + model.Id,
                Body = body,
                Destination = ADMIN_EMAIL
            };
            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            TempData["SuccessMessage"] = "Bug report submitted!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = await _bugService.GetBugReportByIdAsync(id);
            if (entity == null) return HttpNotFound();

            var isAdmin = User.Identity.Name == ADMIN_USERNAME;
            var isCurrentUser = entity.UserId == User.Identity.GetUserId<int>();
            if (!isCurrentUser && !isAdmin) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (isCurrentUser && !isAdmin && entity.IsFixed) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = new BugsEditModel();
            model.BugReport = entity;
            model.IsAdmin = isAdmin;
            model.IsCurrentUser = isCurrentUser;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BugsEditModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Something went wrong while updating the bug report.";
                return View(model);
            }

            var entity = await _bugService.GetBugReportByIdAsync(model.BugReport.Id);
            if (entity == null) return HttpNotFound();

            var isAdmin = User.Identity.Name == ADMIN_USERNAME;
            var isCurrentUser = model.BugReport.UserId == User.Identity.GetUserId<int>();
            if (!isCurrentUser && !isAdmin) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (isCurrentUser && !isAdmin && model.BugReport.IsFixed) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (isCurrentUser)
            {
                entity.Description = model.BugReport.Description;
                entity.UrlWithProblem = model.BugReport.UrlWithProblem;
            }
            if (isAdmin)
            {
                entity.Response = model.BugReport.Response;
                entity.IsFixed = model.BugReport.IsFixed;
            }

            await _bugService.UpdateBugReportAsync(entity);

            // Send email to admin
            var personToEmail = ADMIN_EMAIL;
            var subject = "[SPHINX BUG REPORT] - " + entity.Id;
            var body = "";
            if (isAdmin)
            {
                personToEmail = entity.Member.Email;
                subject += " - Admin Update";
                if (entity.IsFixed)
                {
                    subject += " - FIXED";
                }
                body = RenderRazorViewToString("~/Views/Emails/BugReportAdminUpdate.cshtml", entity);
            }
            else
            {
                subject += " - User Update";
                body = RenderRazorViewToString("~/Views/Emails/BugReportUserUpdate.cshtml", entity);
            }
            var bytes = Encoding.Default.GetBytes(body);
            body = Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = subject,
                Body = body,
                Destination = personToEmail
            };
            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            TempData["SuccessMessage"] = "Bug report updated!";
            return RedirectToAction("Details", new { id = model.BugReport.Id });
        }

        [HttpGet]
        [Authorize(Users = ADMIN_USERNAME)]
        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _bugService.GetBugReportByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Users = ADMIN_USERNAME)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            await _bugService.DeleteBugReportByIdAsync(id);

            TempData["SuccessMessage"] = "Bug report deleted!";
            return RedirectToAction("Index");
        }
    }
}