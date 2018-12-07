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
    [Authorize(Roles = "Administrator, Treasurer")]
    public class DonationsController : BaseController
    {
        private ITreasuryService _treasuryService;

        public DonationsController()
        {
            _treasuryService = new TreasuryService(new Repository<SphinxDbContext>(_db));
        }

        public DonationsController(ITreasuryService treasuryService)
        {
            _treasuryService = treasuryService;
        }

        public async Task<ActionResult> Create(int fid)
        {
            if (fid <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var fundraiser = await _treasuryService.GetFundraiserByIdAsync(fid);

            if (fundraiser == null) return HttpNotFound();

            var model = new Donation
            {
                ReceivedOn = DateTime.UtcNow.FromUtcToCst(),
                FundraiserId = fid,
                Fundraiser = fundraiser
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Donation model)
        {
            if (!ModelState.IsValid) return View(model);

            await _treasuryService.CreateDonationAsync(model);

            TempData["SuccessMessage"] = "Donation created successfully.";
            return RedirectToAction("Details", "Fundraisers", new { id = model.FundraiserId });
        }

        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetDonationByIdAsync(id);

            if (model == null) return HttpNotFound();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(model);
        }

        public async Task<ActionResult> Edit(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetDonationByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Donation model)
        {
            if (!ModelState.IsValid) return View(model);

            await _treasuryService.UpdateDonationAsync(model);

            TempData["SuccessMessage"] = "Donation updated successfully.";
            return RedirectToAction("Details", "Donations", new { id = model.Id });
        }

        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var model = await _treasuryService.GetDonationByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var donation = await _treasuryService.GetDonationByIdAsync(id);

            if (donation == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var fundraiserId = donation.FundraiserId;

            await _treasuryService.DeleteDonationAsync(id);

            TempData["SuccessMessage"] = "Donation deleted successfully.";
            return RedirectToAction("Details", "Fundraisers", new { id = fundraiserId });
        }
    }
}