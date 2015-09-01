namespace Dsp.Areas.Edu.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class MajorsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(await _db.Majors.ToListAsync());
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Create()
        {
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.Name).ToListAsync(),
                "DepartmentId", "Name");
            return View();
        }

        [Authorize(Roles = "Administrator, Academics")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Major model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Majors.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.MajorName + " major was added successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.Majors.FindAsync(id);
            if (model == null) return HttpNotFound();
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.Name).ToListAsync(),
                "DepartmentId", "Name", model.DepartmentId);
            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Major model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.MajorName + " major was updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.Majors.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.Majors.FindAsync(id);
            _db.Majors.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = model.MajorName + " major was deleted successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Assign(int? id)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            if (id != null)
            {
                var member = await UserManager.FindByIdAsync((int)id);
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
                if (User.IsInRole("Administrator") || User.IsInRole("Academics"))
                {
                    ViewBag.UserId = await GetUserIdListAsFullNameAsync();
                }
                else
                {
                    var member = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                    ViewBag.UserId = new SelectList(new List<object>
                    {
                        new { UserId = member.Id, Name = member.FirstName + " " + member.LastName }
                    }, "UserId", "Name");
                }
            }

            ViewBag.MajorId = new SelectList(
                await _db.Majors.OrderBy(c => c.MajorName).ToListAsync(), "MajorId", "MajorName");

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
            if (model.UserId != User.Identity.GetUserId<int>() && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var member = await UserManager.FindByIdAsync(model.UserId);
            if (member.MajorsToMember.Any(m => m.MajorId == model.MajorId && m.DegreeLevel == model.DegreeLevel))
            {
                TempData["FailureMessage"] = "Failed to assign member to major because they are already in that major at that degree level.";
                return RedirectToAction("Assign", new { id = model.UserId });
            }

            _db.MajorsToMembers.Add(model);
            await _db.SaveChangesAsync();

            var major = await _db.Majors.FindAsync(model.MajorId);

            TempData["SuccessMessage"] = member + " was successfully assigned to the " + major.MajorName + " major.";
            return RedirectToAction("Index", "Account", new { area = "Members", userName = member.UserName });
        }

        public async Task<ActionResult> Unassign(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.MajorsToMembers.FindAsync(id);
            if (model == null) return HttpNotFound();
            if (model.UserId != User.Identity.GetUserId<int>() && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(model);
        }

        [HttpPost, ActionName("Unassign"), ValidateAntiForgeryToken]
        public async Task<ActionResult> Unassign(int id)
        {
            var model = await _db.MajorsToMembers.FindAsync(id);
            var name = model.Member.ToString();
            var majorName = model.Major.MajorName;
            if (model.UserId != User.Identity.GetUserId<int>() && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userName = model.Member.UserName;
            _db.MajorsToMembers.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = name + " was successfully unassigned from the " + majorName + " major.";
            return RedirectToAction("Index", "Account", new { area = "Members", userName });
        }
    }
}
