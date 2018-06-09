namespace Dsp.Web.Api
{
    using Data;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    [Authorize]
    [RoutePrefix("api/sobers")]
    public class SobersController : ApiController
    {
        private SphinxDbContext _db;
        private ISoberService _soberService;

        public SobersController()
        {
            _db = new SphinxDbContext();
            _soberService = new SoberService(_db);
        }

        [AllowAnonymous]
        [HttpGet, Route("upcoming")]
        public async Task<IHttpActionResult> Upcoming()
        {
            try
            {
                var upcomingSobers = await _soberService.GetUpcomingSignupsAsync();
                if (upcomingSobers.Any())
                {
                    return Ok(upcomingSobers.Select(m => new
                    {
                        name = m.Member?.ToShortLastNameString() ?? "",
                        when = m.DateOfShift,
                        phone = m.Member?.PhoneNumbers.SingleOrDefault(e => e.Type == "Mobile")?.Number ?? ""
                    }));
                }

                return Ok("No upcoming sober members were found.");
            }
            catch (Exception)
            {
                return BadRequest("API request failed for an unknown reason. Contact your administrator.");
            }
        }
    }
}
