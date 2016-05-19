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
        public async Task<ActionResult> Appointments(AppointmentMessageId? message)
        {
            switch (message)
            {
                case AppointmentMessageId.AppointmentSuccess:
                    ViewBag.SuccessMessage = GetAppointmentMessage(message);
                    break;
                case AppointmentMessageId.AppointmentFailure:
                    ViewBag.FailMessage = GetAppointmentMessage(message);
                    break;
            }

            var positions = await _db.Roles
                .Where(p => !p.IsDisabled)
                .OrderBy(p => p.Type)
                .ThenBy(p => p.DisplayOrder)
                .ToListAsync();
            var semesters = (await GetThisAndNextSemesterListAsync()).ToList();
            var model = new List<AppointmentModel>();

            foreach (var position in positions)
            {
                if (position.Name == "Administrator") continue;
                foreach (var semester in semesters)
                {
                    var leader = await _db.Leaders.SingleOrDefaultAsync(l =>
                        l.SemesterId == semester.SemesterId &&
                        l.RoleId == position.Id) ?? new Leader();
                    model.Add(new AppointmentModel
                    {
                        Leader = new Leader
                        {
                            Position = position,
                            RoleId = position.Id,
                            Semester = semester,
                            SemesterId = semester.SemesterId,
                            UserId = leader.UserId
                        },
                        Users = await GetUserIdListAsFullNameWithNoneNonSelectListAsync(),
                        Alumni = await GetAlumniIdListAsFullNameWithNoneNonSelectListAsync()
                    });
                }
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, President")]
        public async Task<ActionResult> Appointments(IList<AppointmentModel> model)
        {
            AppointmentMessageId? message;

            try
            {
                foreach (var ap in model)
                {
                    // Check if a Leader entry already exists.
                    var leader = await _db.Leaders
                        .SingleOrDefaultAsync(m => m.SemesterId == ap.Leader.SemesterId &&
                                     m.RoleId == ap.Leader.RoleId);

                    if (leader == null)
                    {
                        if (ap.Leader.UserId == 0) continue;
                        ((DspRoleProvider)Roles.Provider).AddUserToRole(ap.Leader.UserId, ap.Leader.RoleId, ap.Leader.SemesterId);
                    }
                    else if (ap.Leader.UserId == 0)
                    {
                        var leaderToRemove = _db.Leaders.Find(leader.RoleId);
                        _db.Leaders.Remove(leaderToRemove);
                    }
                    else if (ap.Leader.UserId != 0)
                    {
                        leader.UserId = ap.Leader.UserId;
                        _db.Entry(leader).State = EntityState.Modified;
                    }
                }
                await _db.SaveChangesAsync();
                message = AppointmentMessageId.AppointmentSuccess;
            }
            catch (Exception)
            {
                message = AppointmentMessageId.AppointmentFailure;
            }

            return RedirectToAction("Appointments", new { Message = message });
        }
        
        private static dynamic GetAppointmentMessage(AppointmentMessageId? message)
        {
            return
                message == AppointmentMessageId.AppointmentSuccess ? "Appointments completed."
                : message == AppointmentMessageId.AppointmentFailure ? "Appointments failed. Please check your appointments and try again."
                : "";
        }

        public enum AppointmentMessageId
        {
            AppointmentSuccess,
            AppointmentFailure
        }
    }
}
