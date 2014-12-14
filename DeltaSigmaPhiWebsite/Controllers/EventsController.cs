namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Service")]
    public class EventsController : BaseController
    {
        private DspContext db = new DspContext();

        public EventsController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index(EventIndexFilterModel model)
        {
            if (model.SelectedSemester == null)
            {
                model.SelectedSemester = GetThisSemestersId();
            }

            var thisSemester = uow.SemesterRepository.SingleById(model.SelectedSemester);
            var previousSemester = uow.SemesterRepository.SelectAll().ToList()
                .Where(s => s.DateEnd < thisSemester.DateStart)
                .OrderBy(s => s.DateEnd).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = thisSemester.DateStart
                };

            model.Events = db.Events.Where(e =>
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
            db.Events.Add(@event);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = db.Events.Find(id);
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
            db.Entry(@event).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = db.Events.Find(id);
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
            var @event = db.Events.Find(id);
            db.Events.Remove(@event);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
