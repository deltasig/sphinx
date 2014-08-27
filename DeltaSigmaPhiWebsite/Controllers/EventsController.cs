namespace DeltaSigmaPhiWebsite.Controllers
{
	using Data.UnitOfWork;
	using Models;
	using Models.Entities;
	using System.Data.Entity;
	using System.Linq;
	using System.Net;
	using System.Web.Mvc;

	[Authorize(Roles = "Administrator, Service")]
	public class EventsController : BaseController
	{
		private DspContext db = new DspContext();

		public EventsController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

		public ActionResult Index()
		{
			return View(db.Events.ToList());
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "EventId,DateTimeOccurred,EventName,DurationHours")] Event @event)
		{
			if (ModelState.IsValid)
			{
                @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
				db.Events.Add(@event);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(@event);
		}

		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Event @event = db.Events.Find(id);
			if (@event == null)
			{
				return HttpNotFound();
			}
			return View(@event);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "EventId,DateTimeOccurred,EventName,DurationHours")] Event @event)
		{
			if (ModelState.IsValid)
            {
                @event.DateTimeOccurred = ConvertCstToUtc(@event.DateTimeOccurred);
				db.Entry(@event).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(@event);
		}

		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Event @event = db.Events.Find(id);
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
			Event @event = db.Events.Find(id);
			db.Events.Remove(@event);
			db.SaveChanges();
			return RedirectToAction("Index");
		}
	}
}
