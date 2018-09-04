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
    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
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
            var bugReports = await _bugService.GetBugReportsAsync(filter.page, filter.pageSize, filter.includeFixed, filter.search);
            filter.count = await _bugService.GetBugReportCountAsync(filter.includeFixed);
            if (filter.pageSize == 0) filter.pageSize = 10;
            filter.pages = (filter.count + filter.pageSize - 1) / filter.pageSize;

            var model = new BugsIndexModel();
            model.BugReports = bugReports;
            model.Filter = filter;

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
            var houseMan = await GetCurrentLeader("House Manager");
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


            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

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

            TempData["SuccessMessage"] = "Bug report updated!";
            return RedirectToAction("Edit", new { id = model.BugReport.Id });
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