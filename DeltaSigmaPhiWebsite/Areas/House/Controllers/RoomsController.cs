namespace DeltaSigmaPhiWebsite.Areas.House.Controllers
{
	using System.Data.Entity;
	using System.Net;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using DeltaSigmaPhiWebsite.Controllers;
	using Entities;

	[Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
	public class RoomsController : BaseController
	{
		public async Task<ActionResult> Index()
		{
			var rooms = await _db.Rooms.ToListAsync();

			return View(rooms);
		}

		public ActionResult Map()
		{
			return View();
		}


		[Authorize(Roles = "Administrator, House Manager")]
		public ActionResult Create()
		{
			return View();
		}

		[Authorize(Roles = "Administrator, House Manager")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(Room room)
		{
			if (!ModelState.IsValid) return View(room);

			_db.Rooms.Add(room);
			await _db.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "Administrator, House Manager")]
		public async Task<ActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var room = await _db.Rooms.FindAsync(id);
			if (room == null)
			{
				return HttpNotFound();
			}
			return View(room);
		}

		[Authorize(Roles = "Administrator, House Manager")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Room room)
		{
			if (!ModelState.IsValid) return View(room);

			_db.Entry(room).State = EntityState.Modified;
			await _db.SaveChangesAsync();
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "Administrator, House Manager")]
		public async Task<ActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			var room = await _db.Rooms.FindAsync(id);
			if (room == null)
			{
				return HttpNotFound();
			}
			return View(room);
		}

		[Authorize(Roles = "Administrator, House Manager")]
		[HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
		public async Task<ActionResult> DeleteConfirmed(int id)
		{
			var room = await _db.Rooms.FindAsync(id);
			_db.Rooms.Remove(room);
			await _db.SaveChangesAsync();
			return RedirectToAction("Index");
		}
	}
}