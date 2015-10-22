namespace Dsp.Areas.Service.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Neophyte, Pledge, Active, Administrator")]
    public class EventsController : BaseController
    {
        public async Task<ActionResult> Index(int? s)
        {
            var thisSemester = await GetThisSemesterAsync();
            if (s == null)
            {
                s = thisSemester.SemesterId;
            }
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var semester = await _db.Semesters.FindAsync(s);
            var previousSemester = (await _db.Semesters
                .Where(sem => sem.DateEnd < semester.DateStart)
                .OrderBy(sem => sem.DateEnd).ToListAsync()).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = semester.DateStart
                };

            var model = new ServiceEventIndexModel
            {
                Semester = semester,
                Events = await _db.Events
                    .Where(e => e.DateTimeOccurred < semester.DateEnd &&
                                e.DateTimeOccurred >= previousSemester.DateEnd)
                    .ToListAsync()
            };

            // Identify valid semesters for dropdown
            var events = await _db.Events.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = new List<Semester>();
            foreach (var sem in allSemesters)
            {
                if (events.Any(i => i.DateTimeOccurred >= sem.DateStart && i.DateTimeOccurred <= sem.DateEnd))
                {
                    semesters.Add(sem);
                }
            }
            // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
            if (semesters.All(sem => sem.SemesterId != thisSemester.SemesterId))
            {
                semesters.Add(thisSemester);
            }

            model.SemesterList = await GetCustomSemesterListAsync(semesters);

            return View(model);
        }

        public async Task<ActionResult> Create(int? s)
        {
            var thisSemester = await GetThisSemesterAsync();
            if (s == null)
            {
                s = thisSemester.SemesterId;
            }
            ViewBag.SemesterId = s;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event model)
        {
            if (!ModelState.IsValid) return View(model);

            if(User.IsInRole("Administrator") || User.IsInRole("Service"))
            {
                model.IsApproved = true;
            }
            else
            {
                model.IsApproved = false;
                // TODO: Email service chairman.
            }
            model.SubmitterId = User.Identity.GetUserId<int>();
            model.DateTimeOccurred = ConvertCstToUtc(model.DateTimeOccurred);
            model.CreatedOn = DateTime.UtcNow;
            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service event created successfully.";
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(model.DateTimeOccurred)).SemesterId
            });
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.Events.FindAsync(id);
            if (model == null) return HttpNotFound();

            var semester = await GetSemestersForUtcDateAsync(model.DateTimeOccurred);
            model.DateTimeOccurred = ConvertUtcToCst(model.DateTimeOccurred);

            ViewBag.SemesterId = semester.SemesterId;
            return View(model);
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.Events.FindAsync(id);
            if (model == null) return HttpNotFound();

            model.DateTimeOccurred = ConvertUtcToCst(model.DateTimeOccurred);
            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(model.DateTimeOccurred)).SemesterId;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(Event model)
        {
            if (!ModelState.IsValid) return View(model);

            model.DateTimeOccurred = ConvertCstToUtc(model.DateTimeOccurred);
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service event modified successfully.";
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(model.DateTimeOccurred)).SemesterId
            });
        }
        
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.Events.FindAsync(id);
            if (model == null) return HttpNotFound();

            var semester = await GetSemestersForUtcDateAsync(model.DateTimeOccurred);
            if (model.ServiceHours.Any())
            {
                TempData["FailureMessage"] = "Failed to delete event because someone has already turned in hours for it.";
                return RedirectToAction("Index", new { s = semester.SemesterId });
            }
            model.DateTimeOccurred = ConvertUtcToCst(model.DateTimeOccurred);

            ViewBag.SemesterId = semester.SemesterId;
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.Events.FindAsync(id);
            _db.Events.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service event deleted successfully.";
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(model.DateTimeOccurred)).SemesterId
            });
        }
    }
}
