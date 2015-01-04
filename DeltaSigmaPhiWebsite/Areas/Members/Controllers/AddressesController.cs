namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using DeltaSigmaPhiWebsite.Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class AddressesController : BaseController
    {
        public async Task<ActionResult> Index(AddressIndexFilterModel model)
        {
            if (model.IsBlank())
            {
                model = new AddressIndexFilterModel
                {
                    Pledges = true,
                    Neophytes = true,
                    Actives = true
                };
                return RedirectToAction("Index", model);
            }

            model.Addresses = await FilterAddressesAsync(model);
            return View(model);
        }

        public async Task<List<Address>> FilterAddressesAsync(AddressIndexFilterModel model)
        {
            var addresses = await _db.Addresses
                .OrderBy(s => s.Member.StatusId)
                .ThenBy(m => m.Member.LastName)
                .ThenBy(a => a.Type).ToListAsync();

            if (!model.Pledges)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Pledge").ToList();
            }
            if (!model.Neophytes)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Neophyte").ToList();
            }
            if (!model.Actives)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Active").ToList();
            }
            if (!model.Alumni)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Alumnus").ToList();
                
            }
            if (!model.Affiliates)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Affiliate").ToList();
                
            }
            if (!model.Released)
            {
                addresses = addresses.Where(a => a.Member.MemberStatus.StatusName != "Released").ToList();
            }

            return addresses;
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
            ViewBag.UserId = new SelectList(await _db.Members.ToListAsync(), "UserId", "UserName", address.UserId);
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
                return WebSecurity.GetUserId(WebSecurity.CurrentUserName) == address.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(await _db.Members.ToListAsync(), "UserId", "UserName", address.UserId);
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
                var line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", 
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
