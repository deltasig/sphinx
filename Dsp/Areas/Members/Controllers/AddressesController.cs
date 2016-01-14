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
    public class AddressesController : BaseController
    {
        public async Task<ActionResult> Index(string s)
        {
            var model = new List<Member>();

            if (!string.IsNullOrEmpty(s))
            {
                if (s == ":all")
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
                            m.Addresses.Any(a => a.Address1.Contains(s)) ||
                            m.Addresses.Any(a => a.Address2.Contains(s)) ||
                            m.Addresses.Any(a => a.City.Contains(s)) ||
                            m.Addresses.Any(a => a.State.Contains(s)) ||
                            m.Addresses.Any(a => a.PostalCode.ToString().Contains(s)) ||
                            m.Addresses.Any(a => a.Country.Contains(s)))
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
            var address = await _db.Addresses.FindAsync(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AddressId,UserId,Type,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(address).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return User.Identity.GetUserId<int>() == address.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        public async Task<FileContentResult> Download()
        {
            var addresses = await _db.Addresses
                .OrderBy(a => a.Member.MemberStatus.StatusId)
                .ThenBy(a => a.Member.LastName)
                .ToListAsync();
            const string header = "First Name, Last Name, Member Status, Address Type, Address 1, Address 2, City, State, Postal Code, Country";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach(var a in addresses)
            {
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", 
                    a.Member.FirstName,
                    a.Member.LastName,
                    a.Member.MemberStatus.StatusName,
                    a.Type,
                    a.Address1,
                    a.Address2,
                    a.City,
                    a.State,
                    a.PostalCode,
                    a.Country);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-addresses.csv");
        }
    }
}
