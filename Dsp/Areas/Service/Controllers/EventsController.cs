namespace Dsp.Areas.Service.Controllers
{
    using Entities;
    using global::Dsp.Controllers;
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
        public async Task<ActionResult> Index(int? s, EventMessageId? message)
        {
            var thisSemester = await base.GetThisSemesterAsync();
            if (s == null)
            {
                s = thisSemester.SemesterId;
            }
            switch (message)
            {
                case EventMessageId.DeleteNotEmptyFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case EventMessageId.CreateSuccess:
                case EventMessageId.EditSuccess:
                case EventMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var semester = await _db.Semesters.FindAsync(s);
            var previousSemester = (await _db.Semesters
                .Where(sem => sem.DateEnd < semester.DateStart)
                .OrderBy(sem => sem.DateEnd).ToListAsync()).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = semester.DateStart
                };

            var model = new ServiceEventIndexModel();
            model.Semester = semester;
            model.Events = await _db.Events
                .Where(e => e.DateTimeOccurred < semester.DateEnd && 
                            e.DateTimeOccurred >= previousSemester.DateEnd)
                .ToListAsync();

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
            var thisSemester = await base.GetThisSemesterAsync();
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
            
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(model.DateTimeOccurred)).SemesterId,
                message = EventMessageId.CreateSuccess
            });
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = await _db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            var semester = await GetSemestersForUtcDateAsync(@event.DateTimeOccurred);
            @event.DateTimeOccurred = base.ConvertUtcToCst(@event.DateTimeOccurred);

            ViewBag.SemesterId = semester.SemesterId;
            return View(@event);
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = await _db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            @event.DateTimeOccurred = base.ConvertUtcToCst(@event.DateTimeOccurred);
            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(@event.DateTimeOccurred)).SemesterId;
            return View(@event);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(Event @event)
        {
            if (!ModelState.IsValid) return View(@event);

            @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
            _db.Entry(@event).State = EntityState.Modified;
            await _db.SaveChangesAsync(); 
            
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(@event.DateTimeOccurred)).SemesterId,
                message = EventMessageId.EditSuccess
            });
        }
        
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = await _db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            var semester = await GetSemestersForUtcDateAsync(@event.DateTimeOccurred);
            if (@event.ServiceHours.Any())
            {
                return RedirectToAction("Index", new
                {
                    s = semester.SemesterId,
                    message = EventMessageId.DeleteNotEmptyFailure
                });
            }
            @event.DateTimeOccurred = base.ConvertUtcToCst(@event.DateTimeOccurred);

            ViewBag.SemesterId = semester.SemesterId;
            return View(@event);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var @event = await _db.Events.FindAsync(id);
            _db.Events.Remove(@event);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(@event.DateTimeOccurred)).SemesterId,
                message = EventMessageId.DeleteSuccess
            });
        }

        public static dynamic GetResultMessage(EventMessageId? message)
        {
            return message == EventMessageId.DeleteNotEmptyFailure ? "Failed to delete event because someone has already turned in hours for it."
                : message == EventMessageId.CreateSuccess ? "Event was created successfully."
                : message == EventMessageId.EditSuccess ? "Event was updated successfully."
                : message == EventMessageId.DeleteSuccess ? "Event was deleted successfully."
                : "";
        }

        public enum EventMessageId
        {
            DeleteNotEmptyFailure,
            CreateSuccess,
            EditSuccess,
            DeleteSuccess
        }
    }
}
