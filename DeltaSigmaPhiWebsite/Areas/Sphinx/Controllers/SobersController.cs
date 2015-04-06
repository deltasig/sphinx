namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Controllers
{
    using App_Start;
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
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

            var signups = await _db.SoberSignups
                .Where(s => s.DateOfShift >= threeAmYesterday)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Manager()
        {
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var vacantSignups = await _db.SoberSignups
                .Where(s => s.DateOfShift >= startOfTodayUtc &&
                            s.UserId == null)
                .OrderBy(s => s.DateOfShift)
                .Include(s => s.SoberType)
                .ToListAsync();
            var model = new SoberManagerModel
            {
                Signups = vacantSignups,
                SignupTypes = await GetSoberTypesSelectList(),
                NewSignup = new SoberSignup(),
                MultiAddModel = new MultiAddSoberSignupModel
                {
                    DriverAmount = 0, 
                    OfficerAmount = 0
                }
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> AddSignup(SoberManagerModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Manager");
            
            model.NewSignup.DateOfShift = ConvertCstToUtc(model.NewSignup.DateOfShift);

            _db.SoberSignups.Add(model.NewSignup);
            await _db.SaveChangesAsync();

            return RedirectToAction("Manager");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> MultiAddSignup(SoberManagerModel model)
        {
            if ((model.MultiAddModel.DriverAmount == 0 && model.MultiAddModel.OfficerAmount == 0) ||
                string.IsNullOrEmpty(model.MultiAddModel.DateString))
                return RedirectToAction("Manager");

            var dateStrings = model.MultiAddModel.DateString.Split(',');

            if (dateStrings.Any())
            {
                foreach (var s in dateStrings)
                {
                    DateTime date;
                    var parsed = DateTime.TryParse(s, out date);
                    if (!parsed) continue;
                    var utcDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
                    var cstDate = base.ConvertUtcToCst(utcDate);
                    var difference = Math.Abs((utcDate - cstDate).Hours);
                    date = date.AddHours(difference);

                    for (var i = 0; i < model.MultiAddModel.DriverAmount; i++)
                    {
                        _db.SoberSignups.Add(new SoberSignup
                        {
                            DateOfShift = date,
                            Type = SoberSignupType.Driver
                        });
                    }
                    for (var i = 0; i < model.MultiAddModel.OfficerAmount; i++)
                    {
                        _db.SoberSignups.Add(new SoberSignup
                        {
                            DateOfShift = date,
                            Type = SoberSignupType.Officer
                        });
                    }
                }
            }

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

            var signupToCancel = await _db.SoberSignups.FindAsync(id);
            _db.SoberSignups.Remove(signupToCancel);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule");
        }

        public async Task<ActionResult> Signup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSignups.FindAsync(id);

            if (signup.UserId != null)
                return RedirectToAction("Schedule");
            if (User.IsInRole("Pledge") && signup.Type == SoberSignupType.Officer)
                return RedirectToAction("Schedule");

            signup.UserId = (await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name)).UserId;
            signup.DateTimeSignedUp = DateTime.UtcNow;

            _db.Entry(signup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { area = "Sphinx", message = "You have successfully signed up to be sober!" });
        }

        public async Task<ActionResult> CancelSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var signup = await _db.SoberSignups.FindAsync(id);
            var oldUserId = signup.UserId;
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            if (signup.UserId == null ||
                (signup.UserId != userId && !User.IsInRole("Administrator") && !User.IsInRole("Sergeant-at-Arms")))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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

            return RedirectToAction("Index", "Home", new
            {
                area = "Sphinx", 
                message = "You have successfully unsigned up to be sober!"
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSignup(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var signup = await _db.SoberSignups.FindAsync(id);
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

            var existingSignup = await _db.SoberSignups.FindAsync(model.SoberSignup.SignupId);

            if (model.SelectedMember <= 0)
                existingSignup.UserId = null;
            else
                existingSignup.UserId = model.SelectedMember;

            existingSignup.Description = model.SoberSignup.Description;
            _db.Entry(existingSignup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new { message = "Successfully updated sober signup." });
        }

        [HttpGet]
        public async Task<ActionResult> Report(SoberReportModel model)
        {
            // Identify semester
            Semester semester;
            if (model.SelectedSemester == null)
                semester = await base.GetThisSemesterAsync();
            else
                semester = await _db.Semesters.FindAsync(model.SelectedSemester);

            // Identify valid semesters for dropdown
            var signups = await _db.SoberSignups.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = new List<Semester>();
            foreach (var s in allSemesters)
            {
                if (signups.Any(i => i.DateOfShift >= s.DateStart && i.DateOfShift <= s.DateEnd))
                {
                    semesters.Add(s);
                }
            }

            // Build model for view
            model.SelectedSemester = semester.SemesterId;
            model.Semester = semester;
            model.SemesterList = await base.GetCustomSemesterListAsync(semesters);
            // Identify members for current semester
            model.Members = await base.GetRosterForSemester(semester);;

            return View(model);
        }
    }
}