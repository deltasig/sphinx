namespace Dsp.Controllers
{
    using Entities;
    using Extensions;
    using MarkdownSharp;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [AllowAnonymous, RequireHttps]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Contacts()
        {
            var thisSemester = await GetThisSemesterAsync();

            if (thisSemester == null) return View();

            var model = await _db.Leaders
                .Where(l => l.SemesterId == thisSemester.SemesterId)
                .ToListAsync();

            return View(model);
        }

        public async Task<ActionResult> Recruitment()
        {
            return View(await _db.ScholarshipApps
                .Where(s => s.IsPublic && s.Type.Name == "Building Better Men Scholarship").ToListAsync());
        }

        public async Task<ActionResult> Scholarships()
        {
            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            var model = new ExternalScholarshipModel
            {
                Applications = await _db.ScholarshipApps.ToListAsync(),
                Types = await _db.ScholarshipTypes.ToListAsync()
            };

            var markdown = new Markdown();
            foreach (var app in model.Applications)
            {
                app.AdditionalText = markdown.Transform(app.AdditionalText);
            }

            return View(model);
        }

        public ActionResult Alumni()
        {
           return RedirectToAction("Index", "Home", new { area = "Alumni" });
        }
        
        public ActionResult About()
        {
            return View();
        }

        public async Task<ActionResult> EmailSoberSchedule()
        {
            var nowUtc = DateTime.UtcNow;
            var nowCst = ConvertUtcToCst(nowUtc);

            var type = await _db.EmailTypes.SingleOrDefaultAsync(e => e.Name == "Sober Schedule");
            if (type == null || string.IsNullOrEmpty(type.Destination)) 
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var emails = await _db.Emails
                .Where(e => 
                    e.EmailTypeId == type.EmailTypeId && 
                    e.Destination == type.Destination)
                .OrderByDescending(e => e.SentOn)
                .ToListAsync();
            var mostRecentEmail = emails.FirstOrDefault();

            // Check if it has been over 24 hours since the last email.
            var noPreviousEmail = mostRecentEmail == null || (nowUtc - mostRecentEmail.SentOn).TotalHours > 24;
            // Check if the current time is between the arbitrary range.
            var isTime = (nowCst.DayOfWeek == DayOfWeek.Friday &&
                          nowCst.Hour >= 16 && nowCst.Hour < 19);
            // If an admin or the sergeant is trying to manually send the email, just allow it.
            var canOverride = (User.IsInRole("Administrator") || User.IsInRole("Sergeant-at-Arms"));

            // Don't send the email if conditions aren't right.
            if ((!isTime || !noPreviousEmail) && !canOverride)
            {
                return Content("Time: " + isTime + ", Email: " + noPreviousEmail);
            }

            // Build Body
            var data = await base.GetThisWeeksSoberSignupsAsync(nowUtc);

            if (!data.Any())
            {
                return Content("No sober signups found; no email sent.");
            }

            var body = base.RenderRazorViewToString("~/Views/Emails/SoberSchedule.cshtml", data);
            var bytes = Encoding.Default.GetBytes(body);
            body = Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = "Sober Schedule: " + 
                nowCst.ToShortDateString() + " - " + nowCst.AddDays(7).ToShortDateString(),
                Body = body,
                Destination = type.Destination
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendTemplatedAsync(message);
            }
            catch (SmtpException e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            var email = new Email
            {
                SentOn = nowUtc, 
                EmailTypeId = type.EmailTypeId, 
                Destination = type.Destination, 
                Body = body
            };

            _db.Emails.Add(email);
            await _db.SaveChangesAsync();

            return Content("OK");
        }
    }
}
