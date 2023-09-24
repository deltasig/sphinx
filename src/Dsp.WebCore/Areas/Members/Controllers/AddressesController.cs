namespace Dsp.WebCore.Areas.Members.Controllers;

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
public class AddressesController : BaseController
{
    public async Task<ActionResult> Index(string s)
    {
        var model = new List<User>();

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
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var address = await Context.Addresses.FindAsync(id);
        if (address == null)
        {
            return NotFound();
        }
        ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", address.UserId);
        return View(address);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Address address)
    {
        if (ModelState.IsValid)
        {
            Context.Entry(address).State = EntityState.Modified;
            await Context.SaveChangesAsync();
            return User.GetUserId() == address.UserId
                ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
        }
        ViewBag.UserId = new SelectList(await UserManager.Users.ToListAsync(), "UserId", "UserName", address.UserId);
        return View(address);
    }

    public async Task<FileContentResult> Download()
    {
        var addresses = await Context.Addresses
            .OrderBy(a => a.User.StatusId)
            .ThenBy(a => a.User.LastName)
            .ToListAsync();
        const string header = "First Name, Last Name, Member Status, Address Type, Address 1, Address 2, City, State, Postal Code, Country";
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var a in addresses)
        {
            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                a.User.FirstName,
                a.User.LastName,
                a.User.Status.StatusName,
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
