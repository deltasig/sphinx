namespace Dsp.Web.Areas.Sobers.Controllers
{
    using Data.Entities;
    using Web.Controllers;
    using Extensions;
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

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class ScheduleController : BaseController
    {
        public async Task<ActionResult> Index(SoberMessage? message)
        {
            SetSoberMessage(message);

            var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date).AddDays(-1).AddHours(3);

            var signups = await _db.SoberSignups
                .Where(s => s.DateOfShift >= threeAmYesterday)
                .OrderBy(s => s.DateOfShift)
                .ToListAsync();
            return View(signups);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Manager(SoberMessage? message)
        {
            SetSoberMessage(message);

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
            if (!ModelState.IsValid) return RedirectToAction("Manager", new { message = SoberMessage.AddSignupFailure });

            model.NewSignup.DateOfShift = ConvertCstToUtc(model.NewSignup.DateOfShift);
            model.NewSignup.CreatedOn = DateTime.UtcNow;

            _db.SoberSignups.Add(model.NewSignup);
            await _db.SaveChangesAsync();

            return RedirectToAction("Manager", new { message = SoberMessage.AddSignupSuccess });
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> MultiAddSignup(SoberManagerModel model)
        {
            if ((model.MultiAddModel.DriverAmount == 0 && model.MultiAddModel.OfficerAmount == 0) ||
                string.IsNullOrEmpty(model.MultiAddModel.DateString))
                return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupFailure });

            var dateStrings = model.MultiAddModel.DateString.Split(',');
            var driverType = await _db.SoberTypes.SingleOrDefaultAsync(t => t.Name == "Driver");
            var officerType = await _db.SoberTypes.SingleOrDefaultAsync(t => t.Name == "Officer");

            if (driverType == null || officerType == null)
            {
                return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupMissingTypesFailure });
            }

            if (dateStrings.Any())
            {
                foreach (var s in dateStrings)
                {
                    DateTime date;
                    var parsed = DateTime.TryParse(s, out date);
                    if (!parsed) continue;
                    var utcDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
                    var cstDate = ConvertUtcToCst(utcDate);
                    var difference = Math.Abs((utcDate - cstDate).Hours);
                    date = date.AddHours(difference);

                    for (var i = 0; i < model.MultiAddModel.DriverAmount; i++)
                    {
                        _db.SoberSignups.Add(new SoberSignup
                        {
                            DateOfShift = date,
                            SoberTypeId = driverType.SoberTypeId,
                            CreatedOn = DateTime.UtcNow
                        });
                    }
                    for (var i = 0; i < model.MultiAddModel.OfficerAmount; i++)
                    {
                        _db.SoberSignups.Add(new SoberSignup
                        {
                            DateOfShift = date,
                            SoberTypeId = officerType.SoberTypeId,
                            CreatedOn = DateTime.UtcNow
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupSuccess });
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> DeleteSignup(int? id, string returnUrl)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = await _db.SoberSignups.FindAsync(id);

            if (model == null) return HttpNotFound();

            _db.SoberSignups.Remove(model);
            await _db.SaveChangesAsync();

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", new { message = SoberMessage.DeleteSignupSuccess });
            return Redirect(returnUrl);
        }

        public async Task<ActionResult> SignupConfirmation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.SoberSignups.FindAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Signup(int id)
        {
            var model = await _db.SoberSignups.FindAsync(id);

            if (model == null) return HttpNotFound();

            if (model.UserId != null)
            {
                return RedirectToAction("Index", new { message = SoberMessage.SignupFailure });
            }
            if (User.IsInRole("Pledge") && model.SoberType.Name == "Officer")
            {
                return RedirectToAction("Index", new { message = SoberMessage.SignupPledgeOfficerFailure });
            }

            model.UserId = (await UserManager.Users.SingleAsync(m => m.UserName == User.Identity.Name)).Id;
            model.DateTimeSignedUp = DateTime.UtcNow;

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { message = SoberMessage.SignupSuccess });
        }

        [HttpGet, Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSignup(int id, string returnUrl)
        {
            var signup = await _db.SoberSignups.FindAsync(id);

            if (signup == null) return HttpNotFound();

            var model = new EditSoberSignupModel
            {
                SoberSignup = signup,
                Members = await GetUserIdListAsFullNameWithNoneAsync()
            };

            if (signup.UserId != null)
                model.SelectedMember = (int)signup.UserId;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> EditSignup(EditSoberSignupModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", new { message = SoberMessage.EditSignupFailure });

            var existingSignup = await _db.SoberSignups.FindAsync(model.SoberSignup.SignupId);

            if (existingSignup == null) return HttpNotFound();

            if (model.SelectedMember <= 0)
            {
                existingSignup.UserId = null;
                existingSignup.DateTimeSignedUp = null;
            }
            else
            {
                existingSignup.UserId = model.SelectedMember;
                existingSignup.DateTimeSignedUp = DateTime.UtcNow;
            }

            existingSignup.Description = model.SoberSignup.Description;
            _db.Entry(existingSignup).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { message = SoberMessage.EditSignupSuccess });
        }

        [HttpGet]
        public async Task<ActionResult> Report(SoberReportModel model)
        {
            var thisSemester = await GetThisSemesterAsync();
            // Identify semester
            Semester semester;
            if (model.SelectedSemester == null)
                semester = thisSemester;
            else
                semester = await _db.Semesters.FindAsync(model.SelectedSemester);

            if (semester == null) return HttpNotFound();

            // Identify valid semesters for dropdown
            var signups = await _db.SoberSignups.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = allSemesters
                .Where(s =>
                    signups.Any(i => i.DateOfShift >= s.DateStart && i.DateOfShift <= s.DateEnd))
                .ToList();
            // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
            if (semesters.All(s => s.SemesterId != thisSemester.SemesterId))
            {
                semesters.Add(thisSemester);
            }

            // Build model for view
            model.SelectedSemester = semester.SemesterId;
            model.Semester = semester;
            model.SemesterList = GetCustomSemesterListAsync(semesters);
            // Identify members for current semester
            model.Members = await GetRosterForSemester(semester);

            return View(model);
        }

        public async Task<ActionResult> Download(int? sid)
        {
            if (sid == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var semester = await _db.Semesters.FindAsync(sid);
            var soberSlots = await _db.SoberSignups
                .Where(s => s.DateOfShift >= semester.DateStart && s.DateOfShift <= semester.DateEnd)
                .ToListAsync();

            const string header = "Shift Date, Day of Week, Type, First Name, Last Name";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var s in soberSlots.OrderBy(s => s.DateOfShift))
            {
                var line = s.DateOfShift.ToString("MM/dd/yyyy, ddd,") +
                    s.SoberType.Name.Replace(",", "") + ",";
                if (s.UserId != null)
                {
                    line += s.Member.FirstName.Replace(",", "") + "," + s.Member.LastName.Replace(",", "");
                }
                else
                {
                    line += ",";
                }
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "DSP Sober Signups - " + semester + ".csv");
        }
    }
}