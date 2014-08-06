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
	public class EventController : BaseController
	{
		private DspContext db = new DspContext();

		public EventController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

		// GET: Event
		public ActionResult Index()
		{
			return View(db.Events.ToList());
		}

		// GET: Event/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: Event/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "EventId,DateTimeOccurred,EventName,DurationHours")] Event @event)
		{
			if (ModelState.IsValid)
			{
				db.Events.Add(@event);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(@event);
		}

		// GET: Event/Edit/5
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

		// POST: Event/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "EventId,DateTimeOccurred,EventName,DurationHours")] Event @event)
		{
			if (ModelState.IsValid)
			{
				db.Entry(@event).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(@event);
		}

		// GET: Event/Delete/5
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

		// POST: Event/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			Event @event = db.Events.Find(id);
			db.Events.Remove(@event);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
