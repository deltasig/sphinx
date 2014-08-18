namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class AddressController : BaseController
    {
        public AddressController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(int? userId)
        {
            if (userId == null)
            {
                var addresses = uow.AddressRepository.SelectAll().ToList()
                    .OrderBy(s => s.Member.StatusId)
                    .ThenBy(m => m.Member.LastName)
                    .ThenBy(a => a.Type);
                ViewBag.Members = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName");
                return View(addresses);
            }
            else
            {
                var addresses = uow.AddressRepository.SelectAll().Where(m => m.UserId == userId).ToList().OrderBy(a => a.Member.LastName);
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
            var address = uow.AddressRepository.SingleById(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AddressId,UserId,Type,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                uow.AddressRepository.Update(address);
                uow.Save();
                return WebSecurity.GetUserId(WebSecurity.CurrentUser.Identity.Name) == address.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.SelectAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }
    }
}
