namespace DeltaSigmaPhiWebsite.Areas.Edu.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class MajorsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.Majors.ToListAsync());
        }

        public async Task<ActionResult> Create()
        {
            ViewBag.DepartmentId = new SelectList(await _db.Departments.OrderBy(c => c.Name).ToListAsync(),
                "DepartmentId", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Major model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Majors.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Major model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

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

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.Majors.FindAsync(id);
            _db.Majors.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        public async Task<ActionResult> Assign()
        {
            if (User.IsInRole("Administrator") && User.IsInRole("Academics"))
            {
                ViewBag.UserId = await base.GetUserIdListAsFullNameAsync();
            }
            else
            {
                var member = await _db.Members.FindAsync(WebSecurity.CurrentUserId);
                ViewBag.UserId = new SelectList(new List<object> 
                { 
                    new { member.UserId, Name = member.FirstName + " " + member.LastName } 
                }, "UserId", "Name");
            }

            ViewBag.MajorId = new SelectList(await _db.Majors.OrderBy(c => c.MajorName).ToListAsync(),
                    "MajorId", "MajorName");

            return View(new MajorToMember());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Assign(MajorToMember model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.MajorsToMembers.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
