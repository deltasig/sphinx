namespace Dsp.WebCore.Api;

using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private DspDbContext _context;
    private ISemesterService _semesterService;
    private IMemberService _memberService;

    public MembersController(DspDbContext context, ISemesterService semesterService,
        IMemberService memberService)
    {
        _context = context;
        _semesterService = semesterService;
        _memberService = memberService;
    }

    [Authorize(Roles = "New, Active, Alumnus")]
    [Route("~/api/members/roster/{sid:int}")]
    public async Task<IActionResult> Roster(int sid)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(sid);
        if (semester == null) return NotFound();

        try
        {
            var roster = await _memberService.GetRosterForSemesterAsync(sid);
            return Ok(roster.Select(m => new
            {
                id = m.Id,
                firstName = m.FirstName,
                lastName = m.LastName
            }));
        }
        catch (Exception)
        {
            return BadRequest("Roster acquisition failed.");
        }
    }

    [Authorize(Roles = "New, Active, Alumnus")]
    [Route("~/api/members/alumni/{sid:int}")]
    public async Task<IActionResult> Alumni(int sid)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(sid);
        if (semester == null) return NotFound();

        try
        {
            var roster = await _memberService.GetAlumniAsync(sid);
            return Ok(roster.Select(m => new
            {
                id = m.Id,
                firstName = m.FirstName,
                lastName = m.LastName
            }));
        }
        catch (Exception)
        {
            return BadRequest("Roster acquisition failed.");
        }
    }
}
