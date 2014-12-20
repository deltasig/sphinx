namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.ViewModels;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
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
    }
}
