namespace Dsp.Web.Api
{
    using Data;
    using Data.Entities;
    using Extensions;
    using Services.Admin;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    [Authorize]
    [RoutePrefix("api/members")]
    public class MembersController : ApiController
    {
        private SphinxDbContext _db;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private IPositionService _positionService;

        public MembersController()
        {
            _db = new SphinxDbContext();
            _semesterService = new SemesterService(_db);
            _memberService = new MemberService(_db);
            _positionService = new PositionService(_db);
        }

        [Authorize(Roles = "Pledge, Active, Alumnus")]
        [HttpGet, Route("roster/{sid:int}")]
        public async Task<IHttpActionResult> Roster(int sid)
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

        [Authorize(Roles = "Pledge, Active, Alumnus")]
        [HttpGet, Route("alumni/{sid:int}")]
        public async Task<IHttpActionResult> Alumni(int sid)
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
}
