using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Repositories;
using Dsp.Services;
using Dsp.Services.Interfaces;
using Dsp.Web.Controllers;
using Dsp.Web.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Dsp.Web.Areas.Treasury.Controllers
{
    [Authorize(Roles = "Neophyte, New, Active, Administrator")]
    public class FundraisersController : BaseController
    {
        private ITreasuryService _treasuryService;

        public FundraisersController()
        {
            _treasuryService = new TreasuryService(new Repository<SphinxDbContext>(_db));
        }

        public FundraisersController(ITreasuryService treasuryService)
        {
            _treasuryService = treasuryService;
        }

        public async Task<ActionResult> Create(int pid)
        {
            if (pid <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cause = await _treasuryService.GetCauseByIdAsync(pid);

            if (cause == null) return HttpNotFound();

            var model = new Fundraiser
            {
                BeginsOn = DateTime.UtcNow.FromUtcToCst(),
                CauseId = pid,
                Cause = cause
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Fundraiser model)
        {
            if (!ModelState.IsValid) return View(model);

            await _treasuryService.CreateFundraiserAsync(model);

            TempData["SuccessMessage"] = "Fundraiser created successfully.";
            return RedirectToAction("Details", "Causes", new { id = model.CauseId });
        }

        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetFundraiserByIdAsync(id);

            if (model == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetFundraiserByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Edit(Fundraiser model)
        {
            if (!ModelState.IsValid) return View(model);

            await _treasuryService.UpdateFundraiserAsync(model);

            TempData["SuccessMessage"] = "Fundraiser updated successfully.";
            return RedirectToAction("Details", "Fundraisers", new { id = model.Id });
        }

        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetFundraiserByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Philanthropy")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var fundraiser = await _treasuryService.GetFundraiserByIdAsync(id);

            if (fundraiser == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var causeId = fundraiser.CauseId;

            await _treasuryService.DeleteFundraiserAsync(id);

            TempData["SuccessMessage"] = "Fundraiser deleted successfully.";
            return RedirectToAction("Details", "Causes", new { id = causeId });
        }
    }
}