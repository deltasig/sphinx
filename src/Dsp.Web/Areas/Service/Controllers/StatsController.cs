using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Repositories;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Dsp.Web.Areas.Service.Models;
using Dsp.Web.Controllers;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Dsp.Web.Areas.Service.Controllers
{
    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator")]
    public class StatsController : BaseController
    {
        private readonly ISemesterService _semesterService;
        private readonly IServiceService _serviceService;
        private readonly IPositionService _positionService;

        public StatsController()
        {
            var repo = new Repository<SphinxDbContext>(_db);
            _semesterService = new SemesterService(repo);
            _serviceService = new ServiceService(repo);
            _positionService = new PositionService(repo);
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

            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Service");
            var navModel = new ServiceNavModel(hasElevatedPermissions, selectedSemester, semesterList);
            var model = new ServiceStatsIndexModel(navModel, memberStats, generalStats);

            return View(model);
        }
    }
}