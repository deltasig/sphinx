namespace Dsp.WebCore.Areas.School.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

[Authorize]
public class MajorsController : BaseController
{
    private readonly IPositionService _positionService;

    public MajorsController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    public async Task<ActionResult> Index()
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        return View(await Context.Majors.ToListAsync());
    }

    [Authorize]
    public async Task<ActionResult> Create()
    {
        ViewBag.DepartmentId = new SelectList(await Context.Departments.OrderBy(c => c.Name).ToListAsync(),
            "DepartmentId", "Name");
        return View();
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(Major model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Majors.Add(model);
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = model.MajorName + " major was added successfully.";
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.Majors.FindAsync(id);
        if (model == null) return NotFound();
        ViewBag.DepartmentId = new SelectList(await Context.Departments.OrderBy(c => c.Name).ToListAsync(),
            "DepartmentId", "Name", model.DepartmentId);
        return View(model);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Major model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Entry(model).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = model.MajorName + " major was updated successfully.";
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var model = await Context.Majors.FindAsync(id);
        if (model == null)
        {
            return NotFound();
        }
        return View(model);
    }

    [Authorize]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var model = await Context.Majors.FindAsync(id);
        Context.Majors.Remove(model);
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = model.MajorName + " major was deleted successfully.";
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Assign(int? id)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        if (id != null)
        {
            var member = await UserManager.FindByIdAsync(id.ToString());
            if (member != null)
            {
                ViewBag.UserName = member.UserName;
                ViewBag.UserId = new SelectList(new List<object>
                {
                    new { UserId = member.Id, Name = member.FirstName + " " + member.LastName }
                }, "UserId", "Name");
            }
        }
        else
        {
            var userId = User.GetUserId();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Academics");
            if (hasElevatedPermissions)
            {
                ViewBag.UserId = await GetUserIdListAsFullNameAsync();
            }
            else
            {
                var member = await UserManager.FindByIdAsync(User.GetUserId().ToString());
                ViewBag.UserId = new SelectList(new List<object>
                {
                    new { UserId = member.Id, Name = member.FirstName + " " + member.LastName }
                }, "UserId", "Name");
            }
        }

        ViewBag.MajorId = new SelectList(
            await Context.Majors.OrderBy(c => c.MajorName).ToListAsync(), "MajorId", "MajorName");

        return View(new MajorToMember());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Assign(MajorToMember model)
    {
        if (!ModelState.IsValid)
        {
            TempData["FailureMessage"] = "Failed to assign member to major because the submission was invalid.  Please try again.";
            return RedirectToAction("Assign", new { id = model.UserId });
        }

        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Academics");
        if (model.UserId != userId && !hasElevatedPermissions)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var member = await UserManager.FindByIdAsync(model.UserId.ToString());
        if (member.Majors.Any(m => m.MajorId == model.MajorId && m.DegreeLevel == model.DegreeLevel))
        {
            TempData["FailureMessage"] = "Failed to assign member to major because they are already in that major at that degree level.";
            return RedirectToAction("Assign", new { id = model.UserId });
        }

        Context.MajorsToMembers.Add(model);
        await Context.SaveChangesAsync();

        var major = await Context.Majors.FindAsync(model.MajorId);

        TempData["SuccessMessage"] = member + " was successfully assigned to the " + major.MajorName + " major.";
        return RedirectToAction("Index", "Account", new { area = "Members", userName = member.UserName });
    }

    public async Task<ActionResult> Unassign(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.MajorsToMembers.FindAsync(id);
        if (model == null) return NotFound();
        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Academics");
        if (model.UserId != userId && !hasElevatedPermissions)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        return View(model);
    }

    [HttpPost, ActionName("Unassign"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Unassign(int id)
    {
        var model = await Context.MajorsToMembers.FindAsync(id);
        var name = model.User.ToString();
        var majorName = model.Major.MajorName;
        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Academics");
        if (model.UserId != userId && !hasElevatedPermissions)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var userName = model.User.UserName;
        Context.MajorsToMembers.Remove(model);
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = name + " was successfully unassigned from the " + majorName + " major.";
        return RedirectToAction("Index", "Account", new { area = "Members", userName });
    }
}
