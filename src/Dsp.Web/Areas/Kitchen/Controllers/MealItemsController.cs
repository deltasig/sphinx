﻿namespace Dsp.Web.Areas.Kitchen.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Exceptions;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Areas.Kitchen.Models;
    using Dsp.Web.Controllers;
    using Microsoft.AspNet.Identity;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.UI;

    [Authorize(Roles = "Alumnus, Active, Neophyte, New")]
    public class MealItemsController : BaseController
    {
        private readonly IMealService _mealService;
        private readonly IPositionService _positionService;

        public MealItemsController()
        {
            var repo = new Repository<SphinxDbContext>(_db);
            _mealService = new MealService(repo);
            _positionService = new PositionService(repo);
        }

        public MealItemsController(IMealService mealService, IPositionService positionService)
        {
            _mealService = mealService;
            _positionService = positionService;
        }

        [OutputCache(Duration = 2592000, VaryByParam = "none", Location = OutputCacheLocation.Server)]
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            var mealItems = await _mealService.GetAllItemsAsync();
            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "House Steward");
            var model = new MealItemIndexModel(mealItems, hasElevatedPermissions);

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MealItem model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = "There was an error with your submission.";
                return View(model);
            }

            try
            {
                await _mealService.CreateItem(model);

                TempData["SuccessMessage"] = $"{model.Name} meal item created!";
                Response.RemoveOutputCacheItem(Url.Action("Index"));
            }
            catch (MealItemAlreadyExistsException ex)
            {
                TempData["FailureMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _mealService.GetItemByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Edit(MealItem model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = "There was an error with your submission.";
                return View(model);
            }

            await _mealService.UpdateItem(model);

            TempData["SuccessMessage"] = $"{model.Name} meal item updated!";
            Response.RemoveOutputCacheItem(Url.Action("Index"));

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> Delete(int id)
        {
            var model = await _mealService.GetItemByIdAsync(id);

            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        [Authorize(Roles = "Administrator, House Steward")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _mealService.DeleteItem(id);

            TempData["SuccessMessage"] = $"Meal item deleted!";
            Response.RemoveOutputCacheItem(Url.Action("Index"));

            return RedirectToAction("Index");
        }
    }
}
