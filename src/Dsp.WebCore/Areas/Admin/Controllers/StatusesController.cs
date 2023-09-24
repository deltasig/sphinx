namespace Dsp.WebCore.Areas.Admin.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.WebCore.Controllers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;
    using System.Threading.Tasks;

    [Authorize(Roles = "Administrator")]
    public class StatusesController : BaseController
    {
        private IStatusService _statusService;

        public StatusesController(DspDbContext context)
        {
            _statusService = new StatusService(context);
        }

        public StatusesController(IStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _statusService.GetAllStatusesAsync());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserType model)
        {
            if (!ModelState.IsValid) return View(model);

            await _statusService.CreateStatus(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            if (id < 1)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var model = await _statusService.GetStatusByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserType model)
        {
            if (!ModelState.IsValid) return View(model);

            await _statusService.UpdateStatus(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            }
            var model = await _statusService.GetStatusByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _statusService.DeleteStatus(id);

            return RedirectToAction("Index");
        }
    }
}