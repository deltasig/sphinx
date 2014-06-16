namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using Models.ViewModels;

    [Authorize]
    public class PhoneNumberController : BaseController
    {
        public PhoneNumberController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(int? userId)
        {
            if (userId == null)
            {
                var addresses = uow.PhoneNumbersRepository.GetAll().ToList()
                    .OrderBy(s => s.Member.StatusId)
                    .ThenBy(m => m.Member.LastName)
                    .ThenBy(a => a.Type);
                ViewBag.Members = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName");
                return View(addresses);
            }
            else
            {
                var addresses = uow.PhoneNumbersRepository.GetAll().Where(m => m.UserId == userId).ToList().OrderBy(a => a.Member.LastName);
                ViewBag.Members = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName");
                return View(addresses);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var phoneNumber = uow.PhoneNumbersRepository.GetById(id);
            if (phoneNumber == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PhoneNumberId,UserId,PhoneNumber1,Type")] PhoneNumber phoneNumber)
        {
            if (ModelState.IsValid)
            {
                uow.PhoneNumbersRepository.Update(phoneNumber);
                uow.Save();
                return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == phoneNumber.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", phoneNumber.UserId);
            return View(phoneNumber);
        }
    }
}
