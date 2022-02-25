namespace Dsp.Web.Areas.Members.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
#if !DEBUG
    using System.Net.Mail;
    using Dsp.Web.Extensions;
#endif

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator")]
    public class IncidentsController : BaseController
    {
        private IIncidentService _incidentService;
        private IPositionService _positionService;
        private ISemesterService _semesterService;
        private IMemberService _memberService;

        public IncidentsController()
        {
            _incidentService = new IncidentService(new Repository<SphinxDbContext>(_db));
            _positionService = new PositionService(new Repository<SphinxDbContext>(_db));
            _semesterService = new SemesterService(new Repository<SphinxDbContext>(_db));
            _memberService = new MemberService(new Repository<SphinxDbContext>(_db));
        }

        public async Task<ActionResult> Index(IncidentsIndexFilterModel filter)
        {
            var serviceResult = await _incidentService.GetIncidentReportsAsync(
                filter.page,
                filter.pageSize,
                filter.resolved,
                filter.unresolved,
                filter.sort,
                filter.s
            );
            var filterResults = serviceResult.Item1;
            var totalPages = serviceResult.Item2;
            var unresolvedCount = serviceResult.Item3;
            var resolvedCount = serviceResult.Item4;

            var model = new IncidentsIndexModel(
                filterResults,
                filter,
                totalPages,
                unresolvedCount,
                resolvedCount
            );

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        public async Task<ActionResult> Details(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var incidentReport = await _incidentService.GetIncidentReportByIdAsync(id);

            if (incidentReport == null) return HttpNotFound();

            var eBoardPositions = await _positionService.GetEboardPositionsAsync();
            var eBoardPositionNames = eBoardPositions.Select(x => x.Name);
            var userId = User.Identity.GetUserId<int>();
            var userRoles = await _positionService.GetCurrentPositionsByUserAsync(userId);
            var model = new IncidentReportDetailsModel
            {
                Report = incidentReport,
                CanEditReport = await _positionService.UserHasAtLeastOnePositionPowerAsync(
                    userId,
                    new[] { "Sergeant-at-Arms", "President" }
                )
            };
            model.CanViewOriginalReport = await _positionService.UserHasAtLeastOnePositionPowerAsync(
                userId,
                new[] { "Sergeant-at-Arms", "President", "Chapter Advisor" }
            );
            model.CanViewInvestigationNotes = await _positionService.UserHasAtLeastOnePositionPowerAsync(
                userId,
                eBoardPositionNames.Concat(new string[] { "Chapter Advisor" }).ToArray()
            );

            return View(model);
        }

        public ActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(IncidentReport model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Something went wrong while submitting the incident report.";
                return View(model);
            }

            model.DateTimeSubmitted = DateTime.UtcNow;
            var submitter = await _memberService.GetMemberByUserNameAsync(User.Identity.Name);
            model.ReportedBy = submitter.Id;
            await _incidentService.CreateIncidentReportAsync(model);

            // Send notification emails to Sergeant-at-Arms and President.
#if !DEBUG
            var currentSemester = await _semesterService.GetCurrentSemesterAsync();
            var saa = await _positionService.GetUserInPositionAsync("Sergeant-at-Arms", currentSemester.Id);
            var president = await _positionService.GetUserInPositionAsync("President", currentSemester.Id);
            var body = RenderRazorViewToString("~/Views/Emails/NewIncidentReport.cshtml", model);
            var message = new IdentityMessage
            {
                Subject = "New Incident Report Submitted: " + model.DateTimeSubmitted.ToString("G"),
                Body = body
            };

            try
            {
                var emailService = new EmailService();
                if (saa != null)
                {
                    message.Destination = saa.Email;
                    await emailService.SendAsync(message);
                }
                if (president != null)
                {
                    message.Destination = president.Email;
                    await emailService.SendAsync(message);
                }
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
#endif
            TempData["SuccessMessage"] = "Incident report submitted.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(int id)
        {
            if (id < 1) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _incidentService.GetIncidentReportByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(IncidentReport model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Something went wrong while updating the incident report.";
                return View(model);
            }

            await _incidentService.UpdateIncidentReportAsync(model);

            TempData["SuccessMessage"] = "Incident report updated.";
            return RedirectToAction("Index");
        }
    }
}
