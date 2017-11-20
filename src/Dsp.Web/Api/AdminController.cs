namespace Dsp.Web.Api
{
    using Data;
    using Data.Entities;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    [Authorize]
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        private SphinxDbContext _db;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private IPositionService _positionService;

        public AdminController()
        {
            _db = new SphinxDbContext();
            _semesterService = new SemesterService(_db);
            _memberService = new MemberService(_db);
            _positionService = new PositionService(_db);
        }

        [Authorize(Roles = "Administrator, President")]
        [HttpGet, Route("appoint/{sid:int}/{pid:int}")]
        public async Task<IHttpActionResult> Appoint(int sid, int pid)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            if (semester == null) return NotFound();
            var position = await _positionService.GetPositionByIdAsync(pid);
            if (position == null) return NotFound();

            try
            {
                var leader = await _db.Leaders
                    .Where(l => l.SemesterId == sid && l.RoleId == pid)
                    .OrderByDescending(l => l.AppointedOn)
                    .FirstOrDefaultAsync();
                if (leader != null) return Ok(new
                {
                    mid = leader.UserId,
                    pid = leader.RoleId,
                    sid = leader.SemesterId,
                    name = leader.Member.FirstName + " " + leader.Member.LastName
                });
                else return Ok("No one is appointed to this position.");
            }
            catch (Exception)
            {
                return BadRequest("Appointment failed. Check if someone is already appointed and remove them first.");
            }
        }

        [Authorize(Roles = "Administrator, President")]
        [HttpPost, Route("appoint"), ResponseType(typeof(Leader))]
        public async Task<IHttpActionResult> Appoint([FromBody] Leader app)
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
        [HttpDelete, Route("appoint"), ResponseType(typeof(Leader))]
        public async Task<IHttpActionResult> Unappoint([FromBody] Leader app)
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
