namespace Dsp.Web.Api
{
    using Data;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [Authorize]
    [RoutePrefix("api/service")]
    public class ServiceController : ApiController
    {
        private SphinxDbContext _db;
        private IServiceService _serviceService;

        public ServiceController()
        {
            _db = new SphinxDbContext();
            _serviceService = new ServiceService(_db);
        }

        [AllowAnonymous]
        [HttpGet, Route("hourstats")]
        public async Task<IHttpActionResult> HourStats(int sid)
        {
            if (sid <= 0) return BadRequest("Based id value provided.");
            try
            {
                var stats = await _serviceService.GetHourStatsBySemesterIdAsync(sid);
                if (stats != null)
                {
                    return Ok(stats);
                }

                return Ok("No stats to report.");
            }
            catch (Exception)
            {
                return BadRequest("API request failed for an unknown reason. Contact your administrator.");
            }
        }
    }
}
