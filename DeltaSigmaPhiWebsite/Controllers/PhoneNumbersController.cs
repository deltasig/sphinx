namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Collections.Generic;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using Models.ViewModels;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class PhoneNumbersController : BaseController
    {
        public PhoneNumbersController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(PhoneIndexFilterModel model)
        {
            if (model.IsBlank())
            {
                model = new PhoneIndexFilterModel
                {
                    Pledges = true,
                    Neophytes = true,
                    Actives = true
                };
            }

            model.PhoneNumbers = FilterPhoneNumbers(model);
            return View(model);
        }

        public List<PhoneNumber> FilterPhoneNumbers(PhoneIndexFilterModel model)
        {
            var phoneNumbers = uow.PhoneNumberRepository.SelectAll()
                .OrderBy(s => s.Member.StatusId)
                .ThenBy(m => m.Member.LastName)
                .ThenBy(a => a.Type).ToList();

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

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var phoneNumber = uow.PhoneNumberRepository.SingleById(id);
            if (phoneNumber == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PhoneNumberId,UserId,Number,Type")] PhoneNumber phoneNumber)
        {
            if (ModelState.IsValid)
            {
                uow.PhoneNumberRepository.Update(phoneNumber);
                uow.Save();
                return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == phoneNumber.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }
    }
}
