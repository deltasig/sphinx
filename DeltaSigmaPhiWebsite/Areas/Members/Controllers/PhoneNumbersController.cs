namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
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
    public class PhoneNumbersController : BaseController
    {
        public async Task<ActionResult> Index(PhoneIndexFilterModel model)
        {
            if (model.IsBlank())
            {
                model = new PhoneIndexFilterModel
                {
                    Pledges = true,
                    Neophytes = true,
                    Actives = true
                };
                return RedirectToAction("Index", model);
            }

            model.PhoneNumbers = await FilterPhoneNumbersAsync(model);
            return View(model);
        }

        public async Task<List<PhoneNumber>> FilterPhoneNumbersAsync(PhoneIndexFilterModel model)
        {
            var phoneNumbers = await _db.PhoneNumbers
                .OrderBy(s => s.Member.StatusId)
                .ThenBy(m => m.Member.LastName)
                .ThenBy(a => a.Type).ToListAsync();

            if (!model.Pledges)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Pledge").ToList();
            }
            if (!model.Neophytes)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Neophyte").ToList();
            }
            if (!model.Actives)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Active").ToList();
            }
            if (!model.Alumni)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Alumnus").ToList();

            }
            if (!model.Affiliates)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Affiliate").ToList();

            }
            if (!model.Released)
            {
                phoneNumbers = phoneNumbers.Where(a => a.Member.MemberStatus.StatusName != "Released").ToList();
            }

            return phoneNumbers;
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
            ViewBag.UserId = new SelectList(await _db.Members.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
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
                return WebSecurity.GetUserId(WebSecurity.CurrentUserName) == phoneNumber.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(await _db.Members.ToListAsync(), "UserId", "UserName", phoneNumber.UserId);
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
                var line = String.Format("{0},{1},{2},{3},{4}",
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
