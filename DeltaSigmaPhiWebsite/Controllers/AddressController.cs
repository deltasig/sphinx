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
    public class AddressController : BaseController
    {
        public AddressController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(int? userId)
        {
            if (userId == null)
            {
                var addresses = uow.AddressesRepository.GetAll().ToList().OrderBy(a => a.Member.LastName);
                ViewBag.Members = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName");
                return View(addresses);
            }
            else
            {
                var addresses = uow.AddressesRepository.GetAll().Where(m => m.UserId == userId).ToList().OrderBy(a => a.Member.LastName);
                ViewBag.Members = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName");
                return View(addresses);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateAddressModel model)
        {
            if (!ModelState.IsValid) return View(model);
            uow.AddressesRepository.Insert(model.Address);
            uow.Save();
            return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == model.Address.UserId
                ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var address = uow.AddressesRepository.GetById(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AddressId,UserId,Type,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                uow.AddressesRepository.Update(address);
                uow.Save();
                return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == address.UserId 
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = uow.AddressesRepository.GetById(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var address = uow.AddressesRepository.GetById(id);
            uow.AddressesRepository.Delete(address);
            uow.Save();
            return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == address.UserId
                ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
        }
    }
}
