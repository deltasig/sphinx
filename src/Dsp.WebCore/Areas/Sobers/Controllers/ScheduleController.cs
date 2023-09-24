namespace Dsp.WebCore.Areas.Sobers.Controllers;

using Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCore.Controllers;

[Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
public class ScheduleController : BaseController
{
    private ISoberService _soberService;
    private IMemberService _memberService;

    public ScheduleController(ISoberService soberService, IMemberService memberService)
    {
        _soberService = soberService;
        _memberService = memberService;
    }

    public async Task<ActionResult> Index(SoberMessage? message)
    {
        SetSoberMessage(message);

        var signups = await _soberService.GetAllFutureSignupsAsync();

        return View(signups);
    }

    [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
    public async Task<ActionResult> Manager(SoberMessage? message)
    {
        SetSoberMessage(message);

        var soberTypes = await _soberService.GetTypesAsync();
        var model = new SoberManagerModel
        {
            Signups = await _soberService.GetFutureVacantSignups(),
            SignupTypes = GetSoberTypesSelectList(soberTypes),
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

        model.NewSignup.DateOfShift = model.NewSignup.DateOfShift.FromCstToUtc();
        model.NewSignup.CreatedOn = DateTime.UtcNow;

        await _soberService.CreateSignupAsync(model.NewSignup);

        return RedirectToAction("Manager", new { message = SoberMessage.AddSignupSuccess });
    }

    [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
    public async Task<ActionResult> MultiAddSignup(SoberManagerModel model)
    {
        var modelInvalid =
            (model.MultiAddModel.DriverAmount == 0 &&
             model.MultiAddModel.OfficerAmount == 0) ||
            string.IsNullOrEmpty(model.MultiAddModel.DateString);
        if (modelInvalid)
        {
            return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupFailure });
        }

        var dateStrings = model.MultiAddModel.DateString.Split(',');
        var driverType = await Context.SoberTypes.SingleOrDefaultAsync(t => t.Name == "Driver");
        var officerType = await Context.SoberTypes.SingleOrDefaultAsync(t => t.Name == "Officer");

        if (driverType == null || officerType == null)
        {
            return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupMissingTypesFailure });
        }

        if (!dateStrings.Any())
        {
            return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupNoDatesFailure });
        }
        else
        {
            var signups = new List<SoberSignup>();
            foreach (var s in dateStrings)
            {
                DateTime date;
                var parsed = DateTime.TryParse(s, out date);
                if (!parsed) continue;
                var utcDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
                var cstDate = utcDate.FromUtcToCst();
                var difference = Math.Abs((utcDate - cstDate).Hours);
                date = date.AddHours(difference);

                for (var i = 0; i < model.MultiAddModel.DriverAmount; i++)
                {
                    signups.Add(new SoberSignup
                    {
                        DateOfShift = date,
                        SoberTypeId = driverType.SoberTypeId,
                        CreatedOn = DateTime.UtcNow
                    });
                }
                for (var i = 0; i < model.MultiAddModel.OfficerAmount; i++)
                {
                    signups.Add(new SoberSignup
                    {
                        DateOfShift = date,
                        SoberTypeId = officerType.SoberTypeId,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }
            await _soberService.CreateSignupsAsync(signups);
        }

        return RedirectToAction("Manager", new { message = SoberMessage.MultiAddSignupSuccess });
    }

    [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
    public async Task<ActionResult> DeleteSignup(int id, string returnUrl)
    {
        if (id <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        try
        {
            await _soberService.DeleteSignupAsync(id);

            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", new { message = SoberMessage.DeleteSignupSuccess });
            return Redirect(returnUrl);
        }
        catch
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
    }

    public async Task<ActionResult> SignupConfirmation(int id)
    {
        if (id <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var model = await Context.SoberSignups.FindAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Signup(int id)
    {
        var model = await Context.SoberSignups.FindAsync(id);

        if (model == null) return NotFound();

        if (model.UserId != null)
        {
            return RedirectToAction("Index", new { message = SoberMessage.SignupFailure });
        }

        var userId = User.GetUserId();
        var user = await _memberService.GetMemberByIdAsync(userId);
        if (user.Status.StatusName == "New" && model.SoberType.Name == "Officer")  // New members can't be sober officer
        {
            return RedirectToAction("Index", new { message = SoberMessage.SignupNewMemberOfficerFailure });
        }

        model.UserId = userId;
        model.DateTimeSignedUp = DateTime.UtcNow;

        Context.Entry(model).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        return RedirectToAction("Index", new { message = SoberMessage.SignupSuccess });
    }

    [HttpGet, Authorize(Roles = "Administrator, Sergeant-at-Arms")]
    public async Task<ActionResult> EditSignup(int id, string returnUrl)
    {
        var signup = await Context.SoberSignups.FindAsync(id);

        if (signup == null) return NotFound();

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

        var existingSignup = await Context.SoberSignups.FindAsync(model.SoberSignup.SignupId);

        if (existingSignup == null) return NotFound();

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
        Context.Entry(existingSignup).State = EntityState.Modified;
        await Context.SaveChangesAsync();

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
            semester = await Context.Semesters.FindAsync(model.SelectedSemester);

        if (semester == null) return NotFound();

        // Identify valid semesters for dropdown
        var signups = await Context.SoberSignups.ToListAsync();
        var allSemesters = await Context.Semesters.ToListAsync();
        var semesters = allSemesters
            .Where(s =>
                signups.Any(i => i.DateOfShift >= s.DateStart && i.DateOfShift <= s.DateEnd))
            .ToList();
        // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
        if (semesters.All(s => s.Id != thisSemester.Id))
        {
            semesters.Add(thisSemester);
        }

        // Build model for view
        model.SelectedSemester = semester.Id;
        model.Semester = semester;
        model.SemesterList = GetSemesterSelectList(semesters);
        // Identify members for current semester
        model.Members = await GetRosterForSemester(semester);

        return View(model);
    }

    public async Task<ActionResult> Download(int? sid)
    {
        if (sid == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var semester = await Context.Semesters.FindAsync(sid);
        var soberSlots = await Context.SoberSignups
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
                line += s.User.FirstName.Replace(",", "") + "," + s.User.LastName.Replace(",", "");
            }
            else
            {
                line += ",";
            }
            sb.AppendLine(line);
        }

        return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "DSP Sober Signups - " + semester + ".csv");
    }

    private SelectList GetSoberTypesSelectList(IEnumerable<SoberType> types)
    {
        var newList = new List<object>();
        foreach (var m in types)
        {
            newList.Add(new
            {
                m.SoberTypeId,
                Text = m.Name
            });
        }
        return new SelectList(newList, "SoberTypeId", "Text");
    }

    public enum SoberMessage
    {
        SignupSuccess,
        SignupFailure,
        AddSignupSuccess,
        AddSignupFailure,
        MultiAddSignupSuccess,
        MultiAddSignupFailure,
        MultiAddSignupMissingTypesFailure,
        CancelSignupSuccess,
        EditSignupSuccess,
        EditSignupFailure,
        SignupNewMemberOfficerFailure,
        DeleteSignupSuccess,
        MultiAddSignupNoDatesFailure,
    }

    public void SetSoberMessage(SoberMessage? message)
    {
        switch (message)
        {
            case SoberMessage.SignupSuccess:
                ViewBag.SuccessMessage = "Sober signup was successful.";
                break;
            case SoberMessage.AddSignupSuccess:
                ViewBag.SuccessMessage = "Successfully added a new sober signup slot.";
                break;
            case SoberMessage.MultiAddSignupSuccess:
                ViewBag.SuccessMessage = "Successfully added multiple new sober signup slots.";
                break;
            case SoberMessage.CancelSignupSuccess:
                ViewBag.SuccessMessage = "Successfully cancelled member's sober signup.";
                break;
            case SoberMessage.EditSignupSuccess:
                ViewBag.SuccessMessage = "Successfully modified sober signup.";
                break;
            case SoberMessage.DeleteSignupSuccess:
                ViewBag.FailMessage = "Successfully deleted sober signup slot.";
                break;
            case SoberMessage.SignupFailure:
                ViewBag.FailMessage = "Sober signup failed.";
                break;
            case SoberMessage.AddSignupFailure:
                ViewBag.FailMessage = "Failed to add sober signup.  Please check the values you entered and try again.";
                break;
            case SoberMessage.MultiAddSignupFailure:
                ViewBag.FailMessage = "Multi-add tool failed because no amounts or dates were provided.";
                break;
            case SoberMessage.MultiAddSignupNoDatesFailure:
                ViewBag.FailMessage = "Multi-add tool failed because no dates were provided.";
                break;
            case SoberMessage.MultiAddSignupMissingTypesFailure:
                ViewBag.FailMessage = "There was an error with the multi-add tool.  " +
                          "This is most likely because the sober types Driver and Officer do not both exist.  " +
                          "If Driver and Officer types exist exactly as they are spelled here and you still " +
                          "get this message, contact Ty Morrow.";
                break;
            case SoberMessage.EditSignupFailure:
                ViewBag.FailMessage = "Failed to modify sober signup.  Please try again or contact your administrator" +
                                      "if the problem persists.";
                break;
            case SoberMessage.SignupNewMemberOfficerFailure:
                ViewBag.FailMessage = "Unfortunately, new members are not allowed to be sober officers.";
                break;
        }
    }
}