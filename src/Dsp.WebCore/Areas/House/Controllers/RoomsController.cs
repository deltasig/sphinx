namespace Dsp.WebCore.Areas.House.Controllers;

using Dsp.Data.Entities;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
            model.Semester = await Context.Semesters.FindAsync(sid);
        }

        var semesters = await Context.Semesters.OrderByDescending(s => s.DateStart).ToListAsync();
        model.SemesterList = base.GetSemesterSelectList(semesters);
        model.sid = model.Semester.Id;
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
            sid = thisSemester.Id;
            ViewBag.Semester = thisSemester.ToString();
        }
        else
        {
            var semester = await Context.Semesters.FindAsync((int)sid);
            sid = semester.Id;
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

        Context.Rooms.Add(room);
        await Context.SaveChangesAsync();
        return RedirectToAction("Index", new { sid = room.SemesterId });
    }

    [Authorize(Roles = "Administrator, House Manager")]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var room = await Context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [Authorize(Roles = "Administrator, House Manager")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(Room room)
    {
        if (!ModelState.IsValid) return View(room);

        Context.Entry(room).State = EntityState.Modified;
        await Context.SaveChangesAsync();
        return RedirectToAction("Index", new { sid = room.SemesterId });
    }

    [Authorize(Roles = "Administrator, House Manager")]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var room = await Context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [Authorize(Roles = "Administrator, House Manager")]
    [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var room = await Context.Rooms.FindAsync(id);
        var sid = room.SemesterId;
        Context.Rooms.Remove(room);
        await Context.SaveChangesAsync();
        return RedirectToAction("Index", new { sid });
    }

    [Authorize(Roles = "Administrator, House Manager")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> GenerateRooms(int? sid)
    {
        if (sid == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var semester = await Context.Semesters.FindAsync(sid);

        // See if rooms already exist.
        if (semester.Rooms.Any())
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }

        // First let's try to copy the previous semesters room entries for convenience.
        var previousSemester = (await Context.Semesters
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
                    SemesterId = semester.Id,
                    Name = r.Name,
                    MaxCapacity = r.MaxCapacity
                };
                Context.Rooms.Add(newRoom);
            }

            await Context.SaveChangesAsync();
            return new StatusCodeResult((int) HttpStatusCode.Accepted);
        }

        // No historical data, we'll just go with the hard-coded values.
        // Add out-of-house
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "Out of House", MaxCapacity = 0 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "RC1", MaxCapacity = 0 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "RC2", MaxCapacity = 0 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "Quad", MaxCapacity = 0 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "TJ", MaxCapacity = 0 });
        // First floor
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "101", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "102", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "103", MaxCapacity = 1 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "104", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "105", MaxCapacity = 4 });
        // Second floor
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "201", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "202", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "203", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "204", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "205", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "206", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "207", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "208", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "209", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "210", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "211", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "212", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "213", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "214", MaxCapacity = 2 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "215", MaxCapacity = 3 });
        Context.Rooms.Add(new Room { SemesterId = (int)sid, Name = "216", MaxCapacity = 3 });

        await Context.SaveChangesAsync();
        return new StatusCodeResult((int) HttpStatusCode.Accepted);
    }

    [Authorize(Roles = "Administrator, House Manager")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Assign(int sid, int mid, int rid, DateTime moveIn, DateTime moveOut)
    {
        var semester = await Context.Semesters.FindAsync(sid);
        if (semester == null)
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var member = await UserManager.FindByIdAsync(mid.ToString());
        if (member == null)
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var room = await Context.Rooms.FindAsync(rid);
        if (room == null)
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        var roomAssignment = new RoomToMember
        {
            RoomId = rid,
            UserId = mid,
            MovedIn = moveIn.FromCstToUtc(),
            MovedOut = moveOut.FromCstToUtc()
        };

        try
        {
            Context.RoomsToMembers.Add(roomAssignment);
            await Context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }

        return new StatusCodeResult((int) HttpStatusCode.Accepted);
    }

    [Authorize(Roles = "Administrator, House Manager")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Unassign(int aid, int sid)
    {
        var roomAssignment = await Context.RoomsToMembers.FindAsync(aid);
        if (roomAssignment == null)
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);

        try
        {
            Context.RoomsToMembers.Remove(roomAssignment);
            await Context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
        }

        return RedirectToAction("Index", new { sid });
    }
}