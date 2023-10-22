namespace Dsp.WebCore.Areas.Service.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Areas.Service.Models;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Area("Service")]
[Authorize]
public class StatsController : BaseController
{
    private readonly ISemesterService _semesterService;
    private readonly IServiceService _serviceService;
    private readonly IPositionService _positionService;

    public StatsController(ISemesterService semesterService, IServiceService serviceService, IPositionService positionService)
    {
        _semesterService = semesterService;
        _serviceService = serviceService;
        _positionService = positionService;
    }

    public async Task<ActionResult> Index(int? sid)
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        Semester selectedSemester = sid == null
            ? currentSemester
            : await _semesterService.GetSemesterByIdAsync((int)sid);

        var memberStats = await _serviceService.GetMemberStatsBySemesterIdAsync(selectedSemester.Id);
        var generalStats = await _serviceService.GetGeneralHistoricalStatsAsync();
        var semestersWithEvents = await _serviceService.GetSemestersWithEventsAsync(currentSemester);
        var semesterList = GetSemesterSelectList(semestersWithEvents);

        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Service");
        var navModel = new ServiceNavModel(hasElevatedPermissions, selectedSemester, semesterList);
        var model = new ServiceStatsIndexModel(navModel, memberStats, generalStats);

        return View(model);
    }
}