using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Repositories;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Dsp.Web.Controllers;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Dsp.Web.Areas.Treasury.Controllers
{
    [Authorize(Roles = "Neophyte, Pledge, Active, Administrator")]
    public class CausesController : BaseController
    {
        private ITreasuryService _causeService;

        public CausesController()
        {
            _causeService = new TreasuryService(new Repository<SphinxDbContext>(_db));
        }

        public CausesController(ITreasuryService causeService)
        {
            _causeService = causeService;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var model = await _causeService.GetAllCausesAsync();

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Cause model)
        {
            if (!ModelState.IsValid) return View(model);

            await _causeService.AddCauseAsync(model);

            TempData["SuccessMessage"] = "Cause created successfully.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _causeService.GetCauseByIdAsync(id);

            if (model == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _causeService.GetCauseByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Edit(Cause model)
        {
            if (!ModelState.IsValid) return View(model);

            await _causeService.UpdateCauseAsync(model);

            TempData["SuccessMessage"] = "Cause updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _causeService.GetCauseByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            await _causeService.DeleteCauseAsync(id);

            TempData["SuccessMessage"] = "Cause deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}