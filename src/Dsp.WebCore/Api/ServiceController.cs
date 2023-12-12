namespace Dsp.WebCore.Api;

using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/service")]
public class ServiceController : ControllerBase
{
    private IServiceService _serviceService;

    public ServiceController(DspDbContext db)
    {
        _serviceService = new ServiceService(db);
    }

    [AllowAnonymous]
    [Route("~/api/service/hourstats")]
    public async Task<IActionResult> HourStats(int sid)
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
