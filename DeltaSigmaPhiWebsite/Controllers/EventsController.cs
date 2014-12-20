namespace DeltaSigmaPhiWebsite.Controllers
{
    using Models.Entities;
    using Models.ViewModels;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Service")]
    public class EventsController : BaseController
    {
        public ActionResult Index(EventIndexFilterModel model)
        {
            if (model.SelectedSemester == null)
            {
                model.SelectedSemester = GetThisSemestersId();
            }

            var thisSemester = _db.Semesters.Find(model.SelectedSemester);
            var previousSemester = _db.Semesters.ToList()
                .Where(s => s.DateEnd < thisSemester.DateStart)
                .OrderBy(s => s.DateEnd).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = thisSemester.DateStart
                };

            model.Events = _db.Events.Where(e =>
                e.DateTimeOccurred < thisSemester.DateEnd &&
                e.DateTimeOccurred >= previousSemester.DateEnd).ToList();
            model.SemesterList = GetSemesterList();

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Event @event)
        {
            if (!ModelState.IsValid) return View(@event);

            @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
            _db.Events.Add(@event);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = _db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event @event)
        {
            if (!ModelState.IsValid) return View(@event);

            @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
            _db.Entry(@event).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = _db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var @event = _db.Events.Find(id);
            _db.Events.Remove(@event);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
