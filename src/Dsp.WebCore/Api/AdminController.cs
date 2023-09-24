namespace Dsp.WebCore.Api
{
    using Amazon.SimpleEmail.Model;
    using Data;
    using Data.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Authorize]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private DspDbContext _context;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private IPositionService _positionService;

        public AdminController(DspDbContext context)
        {
            _context = context;
            _semesterService = new SemesterService(context);
            _memberService = new MemberService(context);
            _positionService = new PositionService(context);
        }

        [Authorize(Roles = "Administrator, President")]
        [Route("~/api/admin/appoint/{sid:int}/{pid:int}")]
        public async Task<IActionResult> Appoint(int sid, int pid)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            if (semester == null) return NotFound();
            var position = await _positionService.GetPositionByIdAsync(pid);
            if (position == null) return NotFound();

            try
            {
                var leader = await _context.UserRoles
                    .Where(l => l.SemesterId == sid && l.RoleId == pid)
                    .OrderByDescending(l => l.AppointedOn)
                    .FirstOrDefaultAsync();
                if (leader != null) return Ok(new
                {
                    mid = leader.UserId,
                    pid = leader.RoleId,
                    sid = leader.SemesterId,
                    name = leader.User.FirstName + " " + leader.User.LastName
                });
                else return Ok("No one is appointed to this position.");
            }
            catch (Exception)
            {
                return BadRequest("Appointment failed. Check if someone is already appointed and remove them first.");
            }
        }

        [Authorize(Roles = "Administrator, President")]
        [Route("~/api/admin/appoint")]
        public async Task<IActionResult> Appoint([FromBody] UserRole app)
        {
            var member = await _memberService.GetMemberByIdAsync(app.UserId);
            if (member == null) return NotFound();
            var semester = await _semesterService.GetSemesterByIdAsync(app.SemesterId);
            if (semester == null) return NotFound();
            var position = await _positionService.GetPositionByIdAsync(app.RoleId);
            if (position == null) return NotFound();

            try
            {
                await _positionService.AppointMemberToPositionAsync(app.UserId, app.RoleId, app.SemesterId);
            }
            catch (Exception)
            {
                return BadRequest("Appointment failed. Check if someone is already appointed and remove them first.");
            }

            return Ok();
        }

        [Authorize(Roles = "Administrator, President")]
        [Route("~/api/admin/unappoint")]
        public async Task<IActionResult> Unappoint([FromBody] UserRole app)
        {
            var member = await _memberService.GetMemberByIdAsync(app.UserId);
            if (member == null) return NotFound();
            var semester = await _semesterService.GetSemesterByIdAsync(app.SemesterId);
            if (semester == null) return NotFound();
            var position = await _positionService.GetPositionByIdAsync(app.RoleId);
            if (position == null) return NotFound();

            try
            {
                await _positionService.RemoveMemberFromPositionAsync(app.UserId, app.RoleId, app.SemesterId);
            }
            catch (Exception)
            {
                return BadRequest("Failed to remove member from position.  Are you sure they're appointed?");
            }

            return Ok();
        }
    }
}
