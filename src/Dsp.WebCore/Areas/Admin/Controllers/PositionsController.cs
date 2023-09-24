namespace Dsp.WebCore.Areas.Admin.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.WebCore.Controllers;
    using Models;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    [Authorize(Roles = "Administrator, President")]
    public class PositionsController : BaseController
    {
        private IPositionService _positionService;
        private ISemesterService _semesterService;

        public PositionsController(DspDbContext context)
        {
            _positionService = new PositionService(context);
            _semesterService = new SemesterService(context);
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var model = await _positionService.GetAllPositionsAsync();
                return View(model);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var position = await _positionService.GetPositionByIdAsync((int)id);
                return View(position);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Role position)
        {
            if (!ModelState.IsValid) return View(position);
            if (string.IsNullOrEmpty(position.Name))
            {
                ViewBag.FailureMessage = "The Name field is required.";
                return View(position);
            }

            try
            {
                await _positionService.CreatePositionAsync(position);
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ViewBag.FailureMessage = ex.Message;
                return View(position);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var position = await _positionService.GetPositionByIdAsync(id);
                return View(position);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Role position)
        {
            if (!ModelState.IsValid) return View(position);

            try
            {
                await _positionService.UpdatePositionAsync(position);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var position = await _positionService.GetPositionByIdAsync(id);
                return View(position);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _positionService.RemovePositionByIdAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
        }

        [HttpGet, Authorize(Roles = "Administrator, President")]
        public async Task<ActionResult> Appointments(int? sid = null)
        {
            try
            {
                Semester semester;
                int semesterId;
                if (sid == null)
                {
                    semester = await _semesterService.GetCurrentSemesterAsync();
                    semesterId = semester.Id;
                }
                else
                {
                    semesterId = (int)sid;
                    semester = await _semesterService.GetSemesterByIdAsync(semesterId);
                }
                var positions = await _positionService.GetAppointmentsAsync(semesterId);
                var semesters = await _semesterService.GetCurrentAndNextSemesterAsync();

                var model = new AppointmentModel
                {
                    Semester = semester,
                    Positions = positions,
                    SemesterList = base.GetSemesterSelectList(semesters)
                };

                return View(model);
            }
            catch (Exception)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }

        }
    }
}
