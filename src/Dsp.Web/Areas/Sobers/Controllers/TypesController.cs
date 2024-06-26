﻿namespace Dsp.Web.Areas.Sobers.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using MarkdownSharp;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Alumnus, Active, New, Neophyte")]
    public class TypesController : BaseController
    {
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Index()
        {
            var types = await _db.SoberTypes.Include(m => m.Signups).ToListAsync();

            var markdown = new Markdown();
            foreach (var type in types)
            {
                type.Description = markdown.Transform(type.Description);
            }

            return View(types);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SoberType soberType)
        {
            if (!ModelState.IsValid) return View(soberType);

            _db.SoberTypes.Add(soberType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }
            return View(soberType);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SoberType soberType)
        {
            if (!ModelState.IsValid) return View(soberType);

            _db.Entry(soberType).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }

            var markdown = new Markdown();
            soberType.Description = markdown.Transform(soberType.Description);

            return View(soberType);
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }
            _db.SoberTypes.Remove(soberType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var soberType = await _db.SoberTypes.FindAsync(id);
            if (soberType == null)
            {
                return HttpNotFound();
            }

            var markdown = new Markdown();
            soberType.Description = markdown.Transform(soberType.Description);

            return View(soberType);
        }
    }
}
