namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize]
    public class AddressController : BaseController
    {
        public AddressController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        // GET: Address
        public ActionResult MyAddresses()
        {
            var addresses = uow.AddressesRepository.GetAll().Where(a => a.Member.UserName == User.Identity.Name).ToList();
            return View(addresses);
        }

        // GET: Address/Details/5
        public ActionResult Details(int? id)
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

        // GET: Address/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName");
            return View();
        }

        // POST: Address/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AddressId,UserId,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                uow.AddressesRepository.Insert(address);
                uow.Save();
                return RedirectToAction("Index", "Account");
            }

            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        // GET: Address/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        // POST: Address/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AddressId,UserId,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                uow.AddressesRepository.Update(address);
                uow.Save();
                return RedirectToAction("Index", "Account");
            }
            ViewBag.UserId = new SelectList(uow.MemberRepository.GetAll(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        // GET: Address/Delete/5
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

        // POST: Address/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Address address = uow.AddressesRepository.GetById(id);
            uow.AddressesRepository.Delete(address);
            uow.Save();
            return RedirectToAction("Index", "Account");
        }

    }
}
