﻿namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using System;
    using System.Data.Entity;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class IncidentsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = await _db.IncidentReports.ToListAsync();
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
            return View(incidentReport);
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
            incidentReport.ReportedBy = (await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name)).UserId;
            
            _db.IncidentReports.Add(incidentReport);
            await _db.SaveChangesAsync();

            // Send notification emails to Sergean-at-Arms and President.
            var currentSemesterId = await GetThisSemestersIdAsync();
            var saaPosition = await _db.Positions.SingleAsync(p => p.PositionName == "Sergeant-at-Arms");
            var presidentPosition = await _db.Positions.SingleAsync(p => p.PositionName == "President");

            var saa = await _db.Leaders
                .SingleAsync(l => 
                    l.SemesterId == currentSemesterId && 
                    l.PositionId == saaPosition.PositionId);
            var president = await _db.Leaders
                .SingleAsync(l => 
                    l.SemesterId == currentSemesterId && 
                    l.PositionId == presidentPosition.PositionId);

            var body = RenderRazorViewToString("~/Views/Emails/NewIncidentReport.cshtml", incidentReport);

            var message = new IdentityMessage
            {
                Subject = "New Incident Report Submitted: " + 
                    base.ConvertUtcToCst(incidentReport.DateTimeSubmitted).ToString("G"),
                Body = body,
                Destination = saa.Member.Email
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendTemplatedAsync(message);
                message.Destination = president.Member.Email;
                await emailService.SendTemplatedAsync(message);
            }
            catch (SmtpException e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

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
