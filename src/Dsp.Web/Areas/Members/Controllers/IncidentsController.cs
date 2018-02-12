namespace Dsp.Web.Areas.Members.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;
    using System.Linq;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class IncidentsController : BaseController
    {
        public async Task<ActionResult> Index(IncidentsIndexFilterModel filter)
        {
            var incidents = await _db.IncidentReports.ToListAsync();
            const int pageSize = 10;
            var filterResults = base.GetFilteredIncidentsList(
                incidents, filter.s, filter.sort, filter.page, filter.unresolved, filter.resolved, pageSize);

            // Build view model with collected data.
            var model = new IncidentsIndexModel
            {
                Incidents = filterResults
            };

            return View(model);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = await _db.IncidentReports.FindAsync(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }
            var eBoardRoles = await _db.Roles.Where(m => m.IsExecutive).Select(e => e.Name).ToListAsync();
            var userRoles = Roles.GetRolesForUser(User.Identity.Name).ToList();
            var model = new IncidentReportDetailsModel();
            model.Report = incidentReport;
            model.CanEditReport = userRoles.Contains("Administrator") || userRoles.Contains("Sergeant-at-Arms") || userRoles.Contains("President");
            model.CanViewOriginalReport = model.CanEditReport; // Could change in the future
            model.CanViewInvestigationNotes = eBoardRoles.Intersect(userRoles).Any();

            return View(model);
        }

        public ActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);
            incidentReport.DateTimeSubmitted = DateTime.UtcNow;
            incidentReport.ReportedBy = (await UserManager.Users.SingleAsync(m => m.UserName == User.Identity.Name)).Id;

            _db.IncidentReports.Add(incidentReport);
            await _db.SaveChangesAsync();

            // Send notification emails to Sergean-at-Arms and President.
#if !DEBUG
            var currentSemesterId = await GetThisSemestersIdAsync();
            var saaPosition = await _db.Roles.SingleAsync(p => p.Name == "Sergeant-at-Arms");
            var presidentPosition = await _db.Roles.SingleAsync(p => p.Name == "President");

            var saa = await _db.Leaders
                .SingleOrDefaultAsync(l =>
                    l.SemesterId == currentSemesterId &&
                    l.RoleId == saaPosition.Id);
            var president = await _db.Leaders
                .SingleOrDefaultAsync(l =>
                    l.SemesterId == currentSemesterId &&
                    l.RoleId == presidentPosition.Id);

            var body = RenderRazorViewToString("~/Views/Emails/NewIncidentReport.cshtml", incidentReport);

            var message = new IdentityMessage
            {
                Subject = "New Incident Report Submitted: " +
                    base.ConvertUtcToCst(incidentReport.DateTimeSubmitted).ToString("G"),
                Body = body
            };

            try
            {
                var emailService = new EmailService();
                if (saa != null)
                {
                    message.Destination = saa.Member.Email;
                    await emailService.SendAsync(message);
                }
                if (president != null)
                {
                    message.Destination = president.Member.Email;
                    await emailService.SendAsync(message);
                }
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
#endif

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = await _db.IncidentReports.FindAsync(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }

            return View(incidentReport);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);

            _db.Entry(incidentReport).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
