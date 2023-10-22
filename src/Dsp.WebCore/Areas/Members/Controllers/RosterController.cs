namespace Dsp.WebCore.Areas.Members.Controllers;

using Dsp.Data.Entities;
using Dsp.WebCore.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Area("Members")]
[Authorize]
public class RosterController : BaseController
{
    [HttpGet]
    public async Task<ActionResult> Index(RosterFilterModel filter)
    {
        Semester semester;
        if (filter.sem == null)
        {
            semester = await base.GetThisSemesterAsync();
            filter.sem = semester.Id;
        }
        else
        {
            semester = await Context.Semesters.FindAsync(filter.sem);
        }

        var members = await base.GetRosterForSemester(semester);
        ViewBag.Sort = filter.sort;
        ViewBag.Order = filter.order;
        ViewBag.SearchTerm = filter.s;
        var filteredResults = base.GetFilteredMembersList(members, filter.s, filter.sort, filter.order);

        var model = new RosterIndexModel
        {
            SelectedSemester = semester.Id,
            Semester = semester,
            Semesters = await base.GetSemesterListAsync(),
            Members = filteredResults
        };

        return View(model);
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> InitiateNewMembers(string message)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        var model = new InitiateNewMembersModel
        {
            NewMembers = await GetNewMemberUserIdListAsFullNameAsync()
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<ActionResult> InitiateNewMembers(InitiateNewMembersModel model)
    {
        var newMembers = await Context.Users
            .Where(m =>
                model.SelectedMemberIds.Contains(m.Id))
            .ToListAsync();
        var activeId = (await Context.UserTypes.SingleAsync(s => s.StatusName == "Active")).StatusId;

        foreach (var m in newMembers)
        {
            m.StatusId = activeId;
            Context.Entry(m).State = EntityState.Modified;
        }

        await Context.SaveChangesAsync();
        TempData["SuccessMessage"] = "New members successfully moved to active status.";
        return RedirectToAction("InitiatePledges");
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> GraduateActives(string message)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        var model = new GraduateActivesModel
        {
            Actives = await GetGraduatingActiveUserIdListAsFullNameAsync()
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<ActionResult> GraduateActives(GraduateActivesModel model)
    {
        var actives = await Context.Users
            .Where(m =>
                model.SelectedMemberIds.Contains(m.Id))
            .ToListAsync();
        var alumnusId = (await Context.UserTypes.SingleAsync(s => s.StatusName == "Alumnus")).StatusId;

        foreach (var p in actives)
        {
            p.StatusId = alumnusId;
            Context.Entry(p).State = EntityState.Modified;
        }

        await Context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Actives successfully moved to alumni status.";
        return RedirectToAction("GraduateActives");
    }

    [HttpGet]
    public async Task<FileContentResult> Download()
    {
        var members = await UserManager.Users
            .OrderBy(m => m.StatusId)
            .ThenBy(m => m.LastName)
            .ToListAsync();
        const string header = "First Name, Last Name, Mobile, Email, Member Status, Pledge Class, Pin, Graduation, Location, Big Bro";
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var m in members)
        {
            var firstName = m.FirstName;
            var lastName = m.LastName;
            var phone = m.PhoneNumber ?? "None";
            var email = m.Email;
            var status = m.Status.StatusName;
            var pledgeClass = m.PledgeClass?.PledgeClassName ?? "None";
            var graduationSemester = m.ExpectedGraduation?.ToString() ?? "None";
            var location = m.RoomString();
            var bigBro = m.BigBro == null ? "None" : m.BigBro.FirstName + " " + m.BigBro.LastName;
            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                firstName,
                lastName,
                phone,
                email,
                status,
                pledgeClass,
                graduationSemester,
                location,
                bigBro);
            sb.AppendLine(line);
        }

        return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-roster.csv");
    }
}
