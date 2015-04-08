namespace DeltaSigmaPhiWebsite.Controllers
{
    using System;
    using Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using App_Start;
    using Microsoft.AspNet.Identity;

    [AllowAnonymous]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Contacts()
        {
            var currentSemester = await GetThisSemesterAsync();

            if (currentSemester == null) return View();

            var model = await _db.Leaders
                .Where(l => l.SemesterId == currentSemester.SemesterId)
                .ToListAsync();

            return View(model);
        }

        public async Task<ActionResult> Recruitment()
        {
            return View(await _db.ScholarshipApps
                .Where(s => s.IsPublic && s.Type.Name == "Building Better Men Scholarship").ToListAsync());
        }

        public async Task<ActionResult> Scholarships(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var model = new ExternalScholarshipModel
            {
                Applications = await _db.ScholarshipApps.Where(s => s.IsPublic).ToListAsync(),
                Types = await _db.ScholarshipTypes.ToListAsync()
            };

            return View(model);
        }

        public async Task<ActionResult> Check()
        {
            return Content("OK");
        }

        public async Task<ActionResult> EmailSoberSchedule()
        {
            var now = DateTime.UtcNow;
            var type = await _db.EmailTypes.SingleOrDefaultAsync(e => e.Name == "Sober Schedule");
            if (type == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mostRecentEmail = await _db.Emails
                .Where(e => e.EmailTypeId == type.EmailTypeId)
                .OrderByDescending(e => e.SentOn)
                .FirstOrDefaultAsync();

            // Check if it has been over 24 hours since the last email.
            var noPreviousEmail = mostRecentEmail == null || (now - mostRecentEmail.SentOn).Hours > 24;
            // Check if the current time is between the arbitrary range.
            var isTime = (now.DayOfWeek == DayOfWeek.Friday &&
                          now.Hour >= 14 && now.Hour <= 16);
            // If an admin or the sergeant is trying to manually send the email, just allow it.
            var canOverride = (User.IsInRole("Administrator") || User.IsInRole("Seargent-at-Arms"));

            // Don't send the email if conditions aren't right.
            if ((!isTime || !noPreviousEmail) && !canOverride) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Build Body
            var body = string.Empty;
            const string destination = "tjm6f4@mst.edu";

            var message = new IdentityMessage
            {
                Subject = "Sober Schedule",
                Body = body,
                Destination = destination
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            var email = new Email
            {
                SentOn = now, 
                EmailTypeId = type.EmailTypeId, 
                Destination = destination, 
                Body = body
            };

            _db.Emails.Add(email);
            await _db.SaveChangesAsync();

            return Content("OK");
        }
    }
}
