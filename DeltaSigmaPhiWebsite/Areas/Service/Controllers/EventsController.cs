namespace DeltaSigmaPhiWebsite.Areas.Service.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using DeltaSigmaPhiWebsite.Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Service")]
    public class EventsController : BaseController
    {
        public async Task<ActionResult> Index(EventIndexFilterModel model)
        {
            if (model.SelectedSemester == null)
            {
                model.SelectedSemester = await GetThisSemestersIdAsync();
            }

            var thisSemester = await _db.Semesters.FindAsync(model.SelectedSemester);
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < thisSemester.DateStart)
                .OrderBy(s => s.DateEnd).ToListAsync()).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = thisSemester.DateStart
                };

            model.Events = await _db.Events
                .Where(e => e.DateTimeOccurred < thisSemester.DateEnd && 
                            e.DateTimeOccurred >= previousSemester.DateEnd)
                .ToListAsync();
            model.SemesterList = await GetSemesterListAsync();

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Event @event)
        {
            if (!ModelState.IsValid) return View(@event);

            @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
            _db.Events.Add(@event);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

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
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Event @event)
        {
            if (!ModelState.IsValid) return View(@event);

            @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
            _db.Entry(@event).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

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
            return View(@event);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var @event = await _db.Events.FindAsync(id);
            _db.Events.Remove(@event);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
