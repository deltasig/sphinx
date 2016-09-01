namespace Dsp.Web.Areas.Admin.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using Extensions;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize(Roles = "Administrator, President")]
    public class PositionsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.Roles.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await RoleManager.FindByIdAsync((int)id);
            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            _db.Roles.Add(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await RoleManager.FindByIdAsync((int)id);
            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            var oldPosition = await _db.Roles.AsNoTracking().SingleAsync(p => p.Id == position.Id);
            // Check if order changed.
            if (oldPosition.DisplayOrder != position.DisplayOrder)
            {
                // Adjust ordering of all positions to accomodate change.
                var positions = await _db.Roles
                    .Where(p =>
                        p.Type == position.Type &&
                        p.Id != position.Id)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();
                positions.Insert(position.DisplayOrder, position);

                for (var i = 0; i < positions.Count; i++)
                {
                    positions[i].DisplayOrder = i;
                    _db.Entry(positions[i]).State = EntityState.Modified;
                }
            }
            else
            {
                _db.Entry(position).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await RoleManager.FindByIdAsync((int)id);
            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var position = await RoleManager.FindByIdAsync(id);
            _db.Roles.Remove(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet, Authorize(Roles = "Administrator, President")]
        public async Task<ActionResult> Appointments(int? sid)
        {
            var semesters = await GetThisAndNextSemesterListAsync();
            Semester semester;
            if (sid == null) semester = semesters.First();
            else semester = semesters.Single(s => s.SemesterId == sid);
            var positions = await _db.Roles
                .Where(p => !p.IsDisabled && p.Name != "Administrator")
                .OrderBy(p => p.Type)
                .ThenBy(p => p.DisplayOrder)
                .ToListAsync();

            var model = new AppointmentModel();
            model.Semester = semester;
            model.Positions = positions;
            model.SemesterList = base.GetCustomSemesterListAsync(semesters);

            return View(model);
        }
    }
}
