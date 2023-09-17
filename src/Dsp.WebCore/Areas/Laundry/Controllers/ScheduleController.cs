namespace Dsp.WebCore.Areas.Laundry.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Services.Exceptions;
    using Dsp.Services.Interfaces;
    using Dsp.WebCore.Controllers;
    using Dsp.WebCore.Extensions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System;
    using System.Threading.Tasks;

    public class ScheduleController : BaseController
    {
        private ILaundryService _laundryService;
        private IPositionService _positionService;

        public ScheduleController(ILaundryService laundryService, IPositionService positionService)
        {
            _laundryService = laundryService;
            _positionService = positionService;
        }

        [HttpGet, Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            // Build Laundry Schedule
            var nowCst = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central Standard Time");
            var existingSignups = await _laundryService.GetSignups(nowCst);
            var schedule = new LaundrySchedule(nowCst, 7, 2, existingSignups);

            var model = new LaundryIndexModel
            {
                Schedule = schedule
            };
            return View(model);
        }

        [HttpPost, Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Reserve(LaundrySignup entity)
        {
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "There was an unknown error with your reservation.  " +
                                             "Contact your administrator if the problem persists.";
                return RedirectToAction("Index");
            }

            entity.UserId = User.GetUserId();
            try
            {
                bool isHouseSteward = await _positionService.UserHasPositionPowerAsync(entity.UserId, "House Steward");
                await _laundryService.Reserve(entity, isHouseSteward ? 4 : 2);
                TempData["SuccessMessage"] = "You have reserved the laundry room for the following time: " + entity.DateTimeShift;
            }
            catch (LaundrySignupsExceededException ex)
            {
                TempData["FailureMessage"] = ex.Message;
            }
            catch (LaundrySignupAlreadyExistsException ex)
            {
                TempData["FailureMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost, Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
        public async Task<ActionResult> Cancel(LaundrySignup entity)
        {
            entity.UserId = User.GetUserId();
            try
            {
                await _laundryService.Cancel(entity);
                TempData["SuccessMessage"] = "You cancelled your reservation for: " + entity.DateTimeShift;
            }
            catch (LaundrySignupNotFoundException ex)
            {
                TempData["FailureMessage"] = ex.Message;
            }
            catch (LaundrySignupPermissionException ex)
            {
                TempData["FailureMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}