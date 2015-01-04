namespace DeltaSigmaPhiWebsite.Areas.Admin.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using DeltaSigmaPhiWebsite.Entities;
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
            return View(await _db.Positions.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await _db.Positions.FindAsync(id);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            _db.Positions.Add(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await _db.Positions.FindAsync(id);
            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            _db.Entry(position).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var position = await _db.Positions.FindAsync(id);
            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var position = await _db.Positions.FindAsync(id);
            _db.Positions.Remove(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Authorize(Roles = "Administrator, President")]
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

            var positions = await _db.Positions
                .Where(p => !p.IsDisabled)
                .OrderBy(p => p.Type)
                .ThenBy(p => p.DisplayOrder)
                .ToListAsync();
            var semesters = (await GetThisAndNextSemesterListAsync()).ToList();
            var model = new List<AppointmentModel>();

            foreach (var position in positions)
            {
                if (position.PositionName == "Administrator") continue;
                foreach (var semester in semesters)
                {
                    var leader = await _db.Leaders.SingleOrDefaultAsync(l =>
                        l.SemesterId == semester.SemesterId &&
                        l.PositionId == position.PositionId) ?? new Leader();
                    model.Add(new AppointmentModel
                    {
                        Leader = new Leader
                        {
                            Position = position,
                            PositionId = position.PositionId,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, President")]
        public async Task<ActionResult> Appointments(IList<AppointmentModel> model)
        {
            AppointmentMessageId? message;

            try
            {
                foreach (var ap in model)
                {
                    // Check if a Leader entry already exists.
                    var leader = await _db.Leaders
                        .SingleAsync(m => m.SemesterId == ap.Leader.SemesterId &&
                                     m.PositionId == ap.Leader.PositionId);

                    if (leader == null)
                    {
                        if (ap.Leader.UserId == 0) continue;
                        ((DspRoleProvider)Roles.Provider).AddUserToRole(ap.Leader.UserId, ap.Leader.PositionId, ap.Leader.SemesterId);
                    }
                    else if (ap.Leader.UserId == 0)
                    {
                        var leaderToRemove = _db.Leaders.Find(leader.LeaderId);
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
