namespace Dsp.Web.Areas.House.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
    public class RoomsController : BaseController
    {
        public async Task<ActionResult> Index(int? sid)
        {
            var model = new RoomIndexModel();
            if (sid == null)
            {
                model.Semester = await base.GetThisSemesterAsync();
            }
            else
            {
                model.Semester = await _db.Semesters.FindAsync(sid);
            }

            var semesters = await _db.Semesters.OrderByDescending(s => s.DateStart).ToListAsync();
            model.SemesterList = base.GetCustomSemesterListAsync(semesters);
            model.sid = model.Semester.SemesterId;
            model.Members = (await base.GetRosterForSemester(model.Semester)).ToList();
            model.Rooms = model.Semester.Rooms;

            return View(model);
        }

        [Authorize(Roles = "Administrator, House Manager")]
        public async Task<ActionResult> Create(int? sid)
        {
            if (sid == null)
            {
                var thisSemester = await base.GetThisSemesterAsync();
                sid = thisSemester.SemesterId;
                ViewBag.Semester = thisSemester.ToString();
            }
            else
            {
                var semester = await _db.Semesters.FindAsync((int)sid);
                sid = semester.SemesterId;
                ViewBag.Semester = semester.ToString();
            }
            var room = new Room { SemesterId = (int)sid };
            return View(room);
        }

        [Authorize(Roles = "Administrator, House Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Room room)
        {
            if (!ModelState.IsValid) return View(room);

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { sid = room.SemesterId });
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
            return RedirectToAction("Index", new { sid = room.SemesterId });
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
            var sid = room.SemesterId;
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { sid });
        }

        [Authorize(Roles = "Administrator, House Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateRooms(int? sid)
        {
            if (sid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Provided 'sid' did not match an existing semester.");
            }
            var semester = await _db.Semesters.FindAsync(sid);

            // See if rooms already exist.
            if (semester.Rooms.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Semester already has rooms entered. Delete all existing rooms before generating.");
            }

            // First let's try to copy the previous semesters room entries for convenience.
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < semester.DateStart)
                .OrderByDescending(s => s.DateStart)
                .ToListAsync())
                .FirstOrDefault();
            if (previousSemester != null && previousSemester.Rooms.Any())
            {
                foreach (var r in previousSemester.Rooms)
                {
                    var newRoom = new Room
                    {
                        SemesterId = semester.SemesterId,
                        Name = r.Name,
                        MaxCapacity = r.MaxCapacity
                    };
                    _db.Rooms.Add(newRoom);
                }

                await _db.SaveChangesAsync();
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            }

            // No historical data, we'll just go with the hard-coded values.
            // Add out-of-house
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "Out of House", MaxCapacity = 0 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "RC1", MaxCapacity = 0 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "RC2", MaxCapacity = 0 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "Quad", MaxCapacity = 0 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "TJ", MaxCapacity = 0 });
            // First floor
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "101", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "102", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "103", MaxCapacity = 1 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "104", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "105", MaxCapacity = 4 });
            // Second floor
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "201", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "202", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "203", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "204", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "205", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "206", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "207", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "208", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "209", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "210", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "211", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "212", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "213", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "214", MaxCapacity = 2 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "215", MaxCapacity = 3 });
            _db.Rooms.Add(new Room { SemesterId = (int)sid, Name = "216", MaxCapacity = 3 });

            await _db.SaveChangesAsync();
            return new HttpStatusCodeResult(HttpStatusCode.Accepted, "Rooms generated successfully.");
        }

        [Authorize(Roles = "Administrator, House Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Assign(int sid, int mid, int rid, DateTime moveIn, DateTime moveOut)
        {
            var semester = await _db.Semesters.FindAsync(sid);
            if (semester == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Provided 'sid' did not match an existing semester.");

            var member = await UserManager.FindByIdAsync(mid);
            if (member == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Provided 'mid' did not match an existing member.");

            var room = await _db.Rooms.FindAsync(rid);
            if (room == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Provided 'rid' did not match an existing room.");

            var roomAssignment = new RoomToMember
            {
                RoomId = rid,
                UserId = mid,
                MovedIn = base.ConvertCstToUtc(moveIn),
                MovedOut = base.ConvertCstToUtc(moveOut)
            };

            try
            {
                _db.RoomsToMembers.Add(roomAssignment);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.InnerException.Message);
            }

            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }

        [Authorize(Roles = "Administrator, House Manager")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Unassign(int aid, int sid)
        {
            var roomAssignment = await _db.RoomsToMembers.FindAsync(aid);
            if (roomAssignment == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Provided 'aid' did not match an existing room assignment.");

            try
            {
                _db.RoomsToMembers.Remove(roomAssignment);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.InnerException.Message);
            }

            return RedirectToAction("Index", new { sid });
        }
    }
}