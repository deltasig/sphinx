namespace Dsp.WebCore.Api;

using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/sobers")]
public class SobersController : ControllerBase
{
    private ISoberService _soberService;

    public SobersController(DspDbContext db)
    {
        _soberService = new SoberService(db);
    }

    [AllowAnonymous]
    [Route("~/api/sobers/upcoming")]
    public async Task<IActionResult> Upcoming()
    {
        try
        {
            var upcomingSobers = await _soberService.GetUpcomingSignupsAsync();
            if (upcomingSobers.Any())
            {
                return Ok(upcomingSobers.Select(m => new
                {
                    name = m.User?.ToShortLastNameString() ?? "",
                    when = m.DateOfShift,
                    phone = m.User?.PhoneNumber ?? ""
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
