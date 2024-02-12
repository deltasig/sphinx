namespace Dsp.WebCore.Areas.Members.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Area("Members")]
[Authorize]
public class RosterController : BaseController
{
    private ISemesterService _semesterService;
    private IMemberService _memberService;

    public RosterController(ISemesterService semesterService, IMemberService memberService)
    {
        _semesterService = semesterService;
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<ActionResult> Index(RosterFilterModel filter)
    {
        Semester semester;
        if (filter.sem == null)
        {
            semester = await _semesterService.GetCurrentSemesterAsync();
            filter.sem = semester.Id;
        }
        else
        {
            semester = await _semesterService.GetSemesterByIdAsync(filter.sem.Value);
        }

        var members = await _memberService.GetRosterForSemesterAsync(semester.Id);
        ViewBag.Sort = filter.sort;
        ViewBag.Order = filter.order;
        ViewBag.SearchTerm = filter.s;
        var filteredResults = GetFilteredMembersList(members, filter.s, filter.sort, filter.order);
        var allSemesters = await _semesterService.GetAllSemestersAsync();
        var allSsemestersSelectList = new SelectList(allSemesters, "Id", "Name", semester.Id);

        var model = new RosterIndexModel
        {
            SelectedSemester = semester.Id,
            Semester = semester,
            Semesters = allSsemestersSelectList,
            Members = filteredResults
        };

        return View(model);
    }

    [HttpGet]
    public async Task<FileContentResult> Download()
    {
        IOrderedEnumerable<Member> members = (await _memberService.GetAllMembersAsync())
            .OrderBy(m => m.LastName);
        const string header = "First Name, Last Name, Mobile, Email, Member Status, Pledge Class, Pin, Graduation, Location, Big Bro";
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var m in members)
        {
            var firstName = m.FirstName;
            var lastName = m.LastName;
            var phone = m.UserInfo.PhoneNumber ?? "None";
            var email = m.Email;
            var status = m.GetStatus(DateTime.UtcNow);
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


    protected virtual IEnumerable<Member> GetFilteredMembersList(
        IEnumerable<Member> members, string s, string sort, string order)
    {
        IEnumerable<Member> filteredResults;

        if (!string.IsNullOrEmpty(s))
        {
            var lcs = s.ToLower();
            filteredResults = members
                .Where(m =>
                    m.FirstName.ToLower().Contains(lcs) ||
                    m.LastName.ToLower().Contains(lcs) ||
                    m.PledgeClass.PledgeClassName.ToLower().Contains(lcs) ||
                    m.ExpectedGraduation.ToString().ToLower() == lcs ||
                    m.RoomString().ToLower() == lcs);
        }
        else
        {
            filteredResults = members;
        }

        switch (sort)
        {
            case "first-name":
                filteredResults = order == "desc"
                    ? filteredResults.OrderByDescending(m => m.FirstName)
                    : filteredResults.OrderBy(m => m.FirstName);
                break;
            case "pledge-class":
                filteredResults = order == "desc"
                    ? filteredResults.OrderByDescending(m => m.PledgeClass.Semester.DateStart)
                    : filteredResults.OrderBy(m => m.PledgeClass.Semester.DateStart);
                break;
            case "final-semester":
                filteredResults = order == "desc"
                    ? filteredResults.OrderByDescending(m => m.ExpectedGraduation.DateStart)
                    : filteredResults.OrderBy(m => m.ExpectedGraduation.DateStart);
                break;
            case "location":
                filteredResults = order == "desc"
                    ? filteredResults.OrderByDescending(m => m.RoomString())
                    : filteredResults.OrderBy(m => m.RoomString());
                break;
            default: // "last-name"
                filteredResults = order == "desc"
                    ? filteredResults.OrderByDescending(m => m.LastName)
                    : filteredResults.OrderBy(m => m.LastName);
                break;
        }

        return filteredResults;
    }
}
