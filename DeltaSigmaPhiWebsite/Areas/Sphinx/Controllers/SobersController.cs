namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Controllers
{
    using App_Start;
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class SobersController : BaseController
    {
        public async Task<ActionResult> Schedule(string message)
        {
            ViewBag.Message = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date).AddDays(-1).AddHours(3);

            var signups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= threeAmYesterday)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Manager()
        {
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var vacantSignups = await _db.SoberSchedule
                .Where(s => s.DateOfShift >= startOfTodayUtc &&
                            s.UserId == null)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();

            return View(vacantSignups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> AddSignup(SoberSignup signup)
        {
            if (!ModelState.IsValid) return RedirectToAction("Manager", "Sphinx");

            signup.DateOfShift = ConvertCstToUtc(signup.DateOfShift);

            _db.SoberSchedule.Add(signup);
            await _db.SaveChangesAsync();

            return RedirectToAction("Manager");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> DeleteSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signupToCancel = await _db.SoberSchedule.FindAsync(id);
            _db.SoberSchedule.Remove(signupToCancel);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule");
        }

        public async Task<ActionResult> Signup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSchedule.FindAsync(id);

            if (signup.UserId != null)
                return RedirectToAction("Schedule");
            if (User.IsInRole("Pledge") && signup.Type == SoberSignupType.Officer)
                return RedirectToAction("Schedule");

            signup.UserId = (await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name)).UserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;

            _db.Entry(signup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { area = "Sphinx", message = "You have successfully signed up to sober drive!" });
        }

        public async Task<ActionResult> CancelSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSchedule.FindAsync(id);
            var oldUserId = signup.UserId;
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            if (signup.UserId == null ||
                (signup.UserId != userId && !User.IsInRole("Administrator") && !User.IsInRole("Sergeant-at-Arms")))
                return RedirectToAction("Schedule", "Sobers");

            signup.UserId = null;
            signup.DateTimeSignedUp = null;

            _db.Entry(signup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            if (WebSecurity.GetUserId(User.Identity.Name) != oldUserId)
                return RedirectToAction("Schedule", "Sobers");

            var member = await _db.Members.FindAsync(oldUserId);
            var currentSemesterId = await GetThisSemestersIdAsync();
            var position = await _db.Positions.SingleAsync(p => p.PositionName == "Sergeant-at-Arms");
            var saa = await _db.Leaders.SingleAsync(l => l.SemesterId == currentSemesterId && l.PositionId == position.PositionId);

            var message = new IdentityMessage
            {
                Subject = "Sphinx - Sober Signup Cancellation: " + member,
                Body = member + " has cancelled his signup for " + signup.DateOfShift.ToShortDateString() + ".",
                Destination = saa.Member.Email
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {

            }

            return RedirectToAction("Schedule", "Sobers");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var signup = await _db.SoberSchedule.FindAsync(id);
            var model = new EditSoberSignupModel
            {
                SoberSignup = signup,
                Members = await GetUserIdListAsFullNameWithNoneAsync()
            };

            if (signup.UserId != null)
                model.SelectedMember = (int)signup.UserId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSignup(EditSoberSignupModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Schedule", new { message = "Failed to update sober signup." });

            var existingSignup = await _db.SoberSchedule.FindAsync(model.SoberSignup.SignupId);

            if (model.SelectedMember <= 0)
                existingSignup.UserId = null;
            else
                existingSignup.UserId = model.SelectedMember;

            _db.Entry(existingSignup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { message = "Successfully updated sober signup." });
        }
    }
}