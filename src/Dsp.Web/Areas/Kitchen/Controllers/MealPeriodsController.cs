namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Areas.Kitchen.Models;
    using Dsp.Web.Controllers;
    using Dsp.Web.Extensions;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, House Steward")]
    public class MealPeriodsController : BaseController
    {
        private IMealService _mealService;

        public MealPeriodsController()
        {
            _mealService = new MealService(new Repository<SphinxDbContext>(_db));
        }

        public MealPeriodsController(IMealService mealService)
        {
            _mealService = mealService;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            var mealPeriods = await _mealService.GetAllPeriodsAsync();
            var model = new MealPeriodIndexModel(mealPeriods);

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MealPeriod model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = "There was an error with your submission.";
                return View(model);
            }

            model.StartTime = model.StartTime.FromUtcToCst();
            model.EndTime = model.EndTime.FromUtcToCst();

            await _mealService.CreatePeriod(model);

            TempData["SuccessMessage"] = $"Meal period created!";

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int id)
        {
            var model = await _mealService.GetPeriodByIdAsync(id);

            if (model == null) return HttpNotFound();

            model.StartTime = model.StartTime.FromUtcToCst();
            model.EndTime = model.EndTime.FromUtcToCst();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MealPeriod model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = "There was an error with your submission.";
                return View(model);
            }

            model.StartTime = model.StartTime.FromUtcToCst();
            model.EndTime = model.EndTime.FromUtcToCst();

            await _mealService.UpdatePeriod(model);

            TempData["SuccessMessage"] = $"{model.Name} meal period updated!";

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            var model = await _mealService.GetPeriodByIdAsync(id);

            if (model == null) return HttpNotFound();

            model.StartTime = model.StartTime.FromUtcToCst();
            model.EndTime = model.EndTime.FromUtcToCst();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _mealService.DeletePeriod(id);

            TempData["SuccessMessage"] = $"Meal period deleted!";

            return RedirectToAction("Index");
        }
    }
}
