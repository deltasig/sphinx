namespace Dsp.Areas.Members.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class PhoneNumbersController : BaseController
    {
        public async Task<ActionResult> Index(string s)
        {
            var model = new List<Member>();

            if (!string.IsNullOrEmpty(s))
            {
                if(s == ":all")
                {
                    model = await _db.Users
                        .OrderBy(m => m.LastName)
                        .ThenBy(m => m.FirstName)
                        .ToListAsync();
                    ViewBag.SearchTerm = "everyone";
                }
                else
                {
                    model = await _db.Users
                        .Where(m =>
                            m.FirstName.Contains(s) ||
                            m.LastName.Contains(s) ||
                            m.PhoneNumbers.Any(p => p.Number.Contains(s)))
                        .ToListAsync();
                    ViewBag.SearchTerm = s;
                }
            }
            
            return View(model);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var phoneNumber = await _db.PhoneNumbers.FindAsync(id);
            if (phoneNumber == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PhoneNumberId,UserId,Number,Type")] PhoneNumber phoneNumber)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(phoneNumber).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return User.Identity.GetUserId<int>() == phoneNumber.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        public async Task<FileContentResult> Download()
        {
            var numbers = await _db.PhoneNumbers
                .OrderBy(a => a.Member.MemberStatus.StatusId)
                .ThenBy(a => a.Member.LastName)
                .ToListAsync();
            const string header = "First Name, Last Name, Member Status, Phone Type, Phone Number";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var n in numbers)
            {
                var line = string.Format("{0},{1},{2},{3},{4}",
                    n.Member.FirstName,
                    n.Member.LastName,
                    n.Member.MemberStatus.StatusName,
                    n.Type,
                    n.Number);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-phone-numbers.csv");
        }
    }
}
