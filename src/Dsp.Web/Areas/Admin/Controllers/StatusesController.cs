﻿namespace Dsp.Web.Areas.Admin.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator")]
    public class StatusesController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _db.MemberStatuses.OrderBy(s => s.StatusId).ToListAsync());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MemberStatus model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.MemberStatuses.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var status = await _db.MemberStatuses.FindAsync(id);
            if (status == null)
            {
                return HttpNotFound();
            }
            return View(status);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MemberStatus model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.MemberStatuses.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var status = await _db.MemberStatuses.FindAsync(id);
            _db.MemberStatuses.Remove(status);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}