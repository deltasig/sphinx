namespace Dsp.Web.Areas.Admin.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
    using Models;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, President")]
    public class PositionsController : BaseController
    {
        private IPositionService _positionService;
        private ISemesterService _semesterService;

        public PositionsController()
        {
            _positionService = new PositionService(new Repository<SphinxDbContext>(_db));
            _semesterService = new SemesterService(new Repository<SphinxDbContext>(_db));
        }

        public PositionsController(IPositionService positionService, ISemesterService semesterService)
        {
            _positionService = positionService;
            _semesterService = semesterService;
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            try
            {
                await _positionService.UpdatePositionAsync(position);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Position position)
        {
            if (!ModelState.IsValid) return View(position);

            try
            {
                await _positionService.UpdatePositionAsync(position);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
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
                    semesterId = semester.SemesterId;
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
                    SemesterList = base.GetCustomSemesterListAsync(semesters)
                };

                return View(model);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }
    }
}
