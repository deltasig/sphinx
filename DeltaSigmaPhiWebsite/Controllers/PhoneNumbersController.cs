﻿namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class PhoneNumbersController : BaseController
    {
        public PhoneNumbersController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(int? userId)
        {
            if (userId == null)
            {
                var addresses = uow.PhoneNumberRepository.SelectAll().ToList()
                    .OrderBy(s => s.Member.StatusId)
                    .ThenBy(m => m.Member.LastName)
                    .ThenBy(a => a.Type);
                ViewBag.Members = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName");
                return View(addresses);
            }
            else
            {
                var addresses = uow.PhoneNumberRepository.SelectAll().Where(m => m.UserId == userId).ToList().OrderBy(a => a.Member.LastName);
                ViewBag.Members = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName");
                return View(addresses);
            }
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