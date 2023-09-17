namespace Dsp.WebCore.Areas.Members.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.WebCore.Controllers;
    using Dsp.WebCore.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
    public class PhoneNumbersController : BaseController
    {
        public async Task<ActionResult> Index(string s)
        {
            var model = new List<Member>();

            if (!string.IsNullOrEmpty(s))
            {
                if (s == ":all")
                {
                    model = await Context.Users
                        .OrderBy(m => m.LastName)
                        .ThenBy(m => m.FirstName)
                        .ToListAsync();
                    ViewBag.SearchTerm = "everyone";
                }
                else
                {
                    model = await Context.Users
                        .Where(m =>
                            m.FirstName.Contains(s) ||
                            m.LastName.Contains(s) ||
                            m.ExtraPhoneNumbers.Any(p => p.PhoneNumber.Contains(s)))
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
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var phoneNumber = await Context.ExtraPhoneNumbers.FindAsync(id);
            if (phoneNumber == null)
            {
                return NotFound();
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ExtraPhoneNumber phoneNumber)
        {
            if (ModelState.IsValid)
            {
                Context.Entry(phoneNumber).State = EntityState.Modified;
                await Context.SaveChangesAsync();
                return User.GetUserId() == phoneNumber.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        public async Task<FileContentResult> Download()
        {
            var numbers = await Context.ExtraPhoneNumbers
                .OrderBy(a => a.User.StatusId)
                .ThenBy(a => a.User.LastName)
                .ToListAsync();
            const string header = "First Name, Last Name, Member Status, Phone Type, Phone Number";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var n in numbers)
            {
                var line = string.Format("{0},{1},{2},{3},{4}",
                    n.User.FirstName,
                    n.User.LastName,
                    n.User.Status.StatusName,
                    n.Type,
                    n.User);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-phone-numbers.csv");
        }
    }
}
