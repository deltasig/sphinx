namespace Dsp.Web.Api
{
    using Data;
    using Services;
    using Services.Interfaces;
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [Authorize]
    [RoutePrefix("api/treasury")]
    public class TreasuryController : ApiController
    {
        private SphinxDbContext _db;
        private ITreasuryService _treasuryService;

        public TreasuryController()
        {
            _db = new SphinxDbContext();
            _treasuryService = new TreasuryService(_db);
        }

        [AllowAnonymous]
        [HttpGet, Route("fundraisers/{fid:int}")]
        public async Task<IHttpActionResult> Fundraisers(int fid)
        {
            var fundraiser = await _treasuryService.GetFundraiserByIdAsync(fid);
            if (fundraiser == null || !fundraiser.IsPublic) return NotFound();

            try
            {
                return Ok(new
                {
                    Id = fundraiser.Id,
                    Description = fundraiser.Description,
                    DonationInstructions = fundraiser.DonationInstructions
                });
            }
            catch (Exception)
            {
                return BadRequest("Roster acquisition failed.");
            }
        }
    }
}
