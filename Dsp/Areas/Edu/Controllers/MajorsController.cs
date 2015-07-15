namespace Dsp.Areas.Edu.Controllers
{
    using Entities;
    using global::Dsp.Controllers;
    using Microsoft.AspNet.Identity;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class MajorsController : BaseController
    {
        public async Task<ActionResult> Index(MajorsMessageId? message)
        {
            switch (message)
            {
                case MajorsMessageId.CreateFailureModelInvalid:
                case MajorsMessageId.UpdateFailureModelInvalid:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case MajorsMessageId.CreateSuccess:
                case MajorsMessageId.UpdateSuccess:
                case MajorsMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

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
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = GetResultMessage(MajorsMessageId.CreateFailureModelInvalid);
                return View(model);
            }

            _db.Majors.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { message = MajorsMessageId.CreateSuccess });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Edit(int? id)
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
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.Name).ToListAsync(),
                "DepartmentId", "Name", model.DepartmentId);
            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Major model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = GetResultMessage(MajorsMessageId.UpdateFailureModelInvalid);
                return View(model);
            }

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { message = MajorsMessageId.UpdateSuccess });
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
            return RedirectToAction("Index", new { message = MajorsMessageId.DeleteSuccess });
        }
        
        public async Task<ActionResult> Assign(int? id, MajorsMessageId? message)
        {
            switch (message)
            {
                case MajorsMessageId.AssignFailureModelInvalid:
                case MajorsMessageId.AssignFailureDuplicate:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case MajorsMessageId.AssignSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

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

            ViewBag.MajorId = new SelectList(await _db.Majors.OrderBy(c => c.MajorName).ToListAsync(),
                    "MajorId", "MajorName");

            return View(new MajorToMember());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Assign(MajorToMember model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Assign", new { id = model.UserId, message = MajorsMessageId.AssignFailureModelInvalid });
            }
            if (model.UserId != User.Identity.GetUserId<int>() && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var member = await UserManager.FindByIdAsync(model.UserId);
            if (member.MajorsToMember.Any(m => m.MajorId == model.MajorId && m.DegreeLevel == model.DegreeLevel))
            {
                return RedirectToAction("Assign", new { id = model.UserId, message = MajorsMessageId.AssignFailureDuplicate });
            }

            _db.MajorsToMembers.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Account", new
            {
                area = "Members",
                userName = member.UserName, 
                majorMessage = MajorsMessageId.AssignSuccess
            });
        }

        public async Task<ActionResult> Unassign(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.MajorsToMembers.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
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
            if (model.UserId != User.Identity.GetUserId<int>() && !User.IsInRole("Administrator") && !User.IsInRole("Academics"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userName = model.Member.UserName;
            _db.MajorsToMembers.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Account", new
            {
                area = "Members", 
                userName,
                majorMessage = MajorsMessageId.UnassignSuccess
            });
        }

        public static dynamic GetResultMessage(MajorsMessageId? message)
        {
            return message == MajorsMessageId.CreateFailureModelInvalid ? "Failed to create major because the submission was invalid.  Please try again."
                : message == MajorsMessageId.CreateSuccess ? "Major creation was successful."
                : message == MajorsMessageId.UpdateFailureModelInvalid ? "Failed to update major because the submission was invalid.  Please try again."
                : message == MajorsMessageId.UpdateSuccess ? "Major update was successful."
                : message == MajorsMessageId.DeleteSuccess ? "Major deletion was successful."
                : message == MajorsMessageId.AssignFailureModelInvalid ? "Failed to assign member to major because the submission was invalid.  Please try again."
                : message == MajorsMessageId.AssignFailureDuplicate ? "Failed to assign member to major because they are already in that major at that degree level."
                : message == MajorsMessageId.AssignSuccess ? "Member was successfully assigned to major."
                : message == MajorsMessageId.UnassignSuccess ? "Member was successfully unassigned from major."
                : "";
        }

        public enum MajorsMessageId
        {
            CreateFailureModelInvalid,
            CreateSuccess,
            UpdateFailureModelInvalid,
            UpdateSuccess,
            DeleteSuccess,
            AssignFailureModelInvalid,
            AssignFailureDuplicate,
            AssignSuccess,
            UnassignSuccess
        }
    }
}
