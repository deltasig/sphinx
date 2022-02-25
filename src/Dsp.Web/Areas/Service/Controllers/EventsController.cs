namespace Dsp.Web.Areas.Service.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Neophyte, New, Active, Alumnus, Administrator")]
    public class EventsController : BaseController
    {
        private readonly ISemesterService _semesterService;
        private readonly IServiceService _serviceService;
        private readonly IPositionService _positionService;

        public EventsController()
        {
            var db = new Repository<SphinxDbContext>(_db);
            _semesterService = new SemesterService(db);
            _serviceService = new ServiceService(db);
            _positionService = new PositionService(db);
        }

        public EventsController(ISemesterService semesterService, IServiceService serviceService, IPositionService positionService)
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

            var serviceEvents = await _serviceService.GetEventsForSemesterAsync(selectedSemester);
            foreach (var se in serviceEvents)
            {
                se.DateTimeOccurred = _semesterService.ConvertUtcToCst(se.DateTimeOccurred);
            }

            var semestersWithEvents = await _serviceService.GetSemestersWithEventsAsync(currentSemester);
            var semesterList = GetSemesterSelectList(semestersWithEvents);
            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Service");
            var navModel = new ServiceNavModel(hasElevatedPermissions, selectedSemester, semesterList);
            var model = new ServiceEventIndexModel(navModel, serviceEvents);

            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];
            return View(model);
        }

        public async Task<ActionResult> Create(int sid)
        {
            if (sid <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            var model = new ServiceEvent
            {
                DateTimeOccurred = _semesterService.ConvertUtcToCst(semester.DateStart),
                SemesterId = semester.Id
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ServiceEvent model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Service");
            if (hasElevatedPermissions)
            {
                model.IsApproved = true;
            }
            else
            {
                model.IsApproved = false;
                // TODO: Email service chairman.
            }
            model.SubmitterId = userId;
            model.DateTimeOccurred = _semesterService.ConvertCstToUtc(model.DateTimeOccurred);
            model.CreatedOn = DateTime.UtcNow;
            var eventSemester = await _semesterService.GetSemesterByUtcDateTimeAsync(model.DateTimeOccurred);
            model.SemesterId = eventSemester.Id;

            await _serviceService.CreateEventAsync(model);

            TempData[SuccessMessageKey] = "Service event created successfully.";
            return RedirectToAction("Index", new { sid = eventSemester.Id });
        }

        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _serviceService.GetEventByIdAsync(id);
            if (model == null) return HttpNotFound();

            model.DateTimeOccurred = _semesterService.ConvertUtcToCst(model.DateTimeOccurred);

            return View(model);
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _serviceService.GetEventByIdAsync(id);
            if (model == null) return HttpNotFound();

            model.DateTimeOccurred = _semesterService.ConvertUtcToCst(model.DateTimeOccurred);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(ServiceEvent model)
        {
            if (!ModelState.IsValid) return View(model);

            model.DateTimeOccurred = _semesterService.ConvertCstToUtc(model.DateTimeOccurred);
            var eventSemester = await _semesterService.GetSemesterByUtcDateTimeAsync(model.DateTimeOccurred);
            model.SemesterId = eventSemester.Id;

            await _serviceService.UpdateEventAsync(model);

            TempData[SuccessMessageKey] = "Service event modified successfully.";
            return RedirectToAction("Index", new { sid = eventSemester.Id });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _serviceService.GetEventByIdAsync(id);
            if (model == null) return HttpNotFound();

            if (model.ServiceHours.Any())
            {
                TempData[FailureMessageKey] = "Failed to delete event because someone has already turned in hours for it.";
                return RedirectToAction("Index", new { sid = model.SemesterId });
            }

            model.DateTimeOccurred = _semesterService.ConvertUtcToCst(model.DateTimeOccurred);

            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _serviceService.GetEventByIdAsync(id);
            var eventSemesterId = model.SemesterId;

            await _serviceService.DeleteEventByIdAsync(id);
            TempData[SuccessMessageKey] = "Service event deleted successfully.";

            return RedirectToAction("Index", new { sid = eventSemesterId });
        }
    }
}
