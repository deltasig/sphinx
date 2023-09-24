namespace Dsp.WebCore.Areas.Kitchen.Controllers;

using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Services;
using Dsp.Services.Exceptions;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Areas.Kitchen.Models;
using Dsp.WebCore.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize]
public class MealItemsController : BaseController
{
    private readonly IMealService _mealService;
    private readonly IPositionService _positionService;
    private readonly UserManager<User> _userManager;

    public MealItemsController(DspDbContext context, UserManager<User> userManager)
    {
        _mealService = new MealService(context);
        _positionService = new PositionService(context);
        _userManager = userManager;
    }

    public async Task<ActionResult> Index()
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailMessage = TempData["FailureMessage"];

        var mealItems = await _mealService.GetAllItemsAsync();
        var currentUser = await _userManager.GetUserAsync(HttpContext.User);
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(currentUser.Id, "House Steward");
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
        }
        catch (MealItemAlreadyExistsException ex)
        {
            TempData["FailureMessage"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Edit(int id)
    {
        var model = await _mealService.GetItemByIdAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(MealItem model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.FailMessage = "There was an error with your submission.";
            return View(model);
        }

        await _mealService.UpdateItem(model);

        TempData["SuccessMessage"] = $"{model.Name} meal item updated!";

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Delete(int id)
    {
        var model = await _mealService.GetItemByIdAsync(id);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        await _mealService.DeleteItem(id);

        TempData["SuccessMessage"] = $"Meal item deleted!";

        return RedirectToAction("Index");
    }
}
