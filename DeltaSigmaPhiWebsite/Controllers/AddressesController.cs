namespace DeltaSigmaPhiWebsite.Controllers
{
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class AddressesController : BaseController
    {
        private readonly DspDbContext _db = new DspDbContext();
        
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
            var addresses = _db.Addresses.ToList()
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
            var address = _db.Addresses.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(_db.Members.ToList(), "UserId", "UserName", address.UserId);
            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AddressId,UserId,Type,Address1,Address2,City,State,PostalCode,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                _db.Addresses.Attach(address);
                _db.Entry(address).State = EntityState.Modified;
                _db.SaveChanges();
                return WebSecurity.GetUserId(WebSecurity.CurrentUserName) == address.UserId
                    ? RedirectToAction("Index", "Account") : RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(_db.Members.ToList(), "UserId", "UserName", address.UserId);
            return View(address);
        }
    }
}
