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
    public class AddressesController : BaseController
    {
        public AddressesController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }
        
        public ActionResult Index(AddressIndexFilterModel model)
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

            model.Addresses = FilterAddresses(model);
            return View(model);
        }

        public List<Address> FilterAddresses(AddressIndexFilterModel model)
        {
            var addresses = uow.AddressRepository.SelectAll()
                .OrderBy(s => s.Member.StatusId)
                .ThenBy(m => m.Member.LastName)
                .ThenBy(a => a.Type).ToList();

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
