namespace Dsp.WebCore.Areas.Service.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.WebCore.Areas.Service.Models;
    using Dsp.WebCore.Controllers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    [Authorize(Roles = "Administrator, Service")]
    public class AmendmentsController : BaseController
    {
        private readonly IMemberService _memberService;
        private readonly ISemesterService _semesterService;
        private readonly IServiceService _serviceService;

        public async Task<ActionResult> Index(int? sid)
        {
            var currentSemester = await GetThisSemesterAsync();
            Semester selectedSemester = sid == null
                ? currentSemester
                : await _semesterService.GetSemesterByIdAsync((int)sid);

            var serviceHourAmendments = await _serviceService.GetHoursAmendmentsBySemesterIdAsync((int)sid);
            var serviceEventAmendments = await _serviceService.GetEventAmendmentsBySemesterIdAsync((int)sid);
            var semesterList = GetSemesterSelectList(await _serviceService.GetSemestersWithEventsAsync(currentSemester));
            var navModel = new ServiceNavModel(true, selectedSemester, semesterList);
            var model = new ServiceAmendmentModel(navModel, serviceHourAmendments, serviceEventAmendments);

            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];
            return View(model);
        }

        public async Task<ActionResult> AmendHours(int? sid)
        {
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            Semester semester = sid == null
                ? await _semesterService.GetCurrentSemesterAsync()
                : await _semesterService.GetSemesterByIdAsync((int)sid);

            var model = new AddServiceHourAmendmentModel
            {
                Amendment = new ServiceHourAmendment
                {
                    SemesterId = semester.Id
                },
                Semester = semester
            };
            var members = await GetRosterForSemester(semester);
            var memberList = new List<object>();
            foreach (Member member in members.OrderBy(m => m.LastName))
            {
                memberList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName +
                        " (" + member.LivingAssignmentForSemester(semester.Id) + ")"
                });
            }
            model.Members = new SelectList(memberList, "UserId", "Name");

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AmendHours(AddServiceHourAmendmentModel model)
        {
            if (model.Amendment.AmountHours.Equals(0) ||
                model.Amendment.AmountHours < -50 ||
                model.Amendment.AmountHours > 50)
            {
                TempData[FailureMessageKey] = "Please enter hours within the range -50 and 50 (excluding 0) in increments of 0.5.";
                return RedirectToAction("AmendHours", new { sid = model.Amendment.SemesterId });
            }
            // Adjust hours to nearest half hour.
            var fraction = (model.Amendment.AmountHours % 1) * 10;
            if (!fraction.Equals(5))
            {
                model.Amendment.AmountHours = Math.Floor(model.Amendment.AmountHours);
            }

            await _serviceService.CreateHoursAmendmentAsync(model.Amendment);

            TempData[SuccessMessageKey] = "Service amendment added successfully.";

            return RedirectToAction("Index", new { sid = model.Amendment.SemesterId });
        }

        public async Task<ActionResult> UnamendHours(int aid)
        {
            if (aid <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            var model = await _serviceService.GetHoursAmendmentByIdAsync(aid);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UnamendHours(ServiceHourAmendment entity)
        {
            if (entity.Id <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

            var semesterId = entity.SemesterId;
            await _serviceService.DeleteHoursAmendmentByIdAsync(entity.Id);

            TempData[SuccessMessageKey] = "Service amendment deleted successfully.";

            return RedirectToAction("Index", new { sid = semesterId });
        }

        public async Task<ActionResult> AmendEvents(int? sid)
        {
            ViewBag.FailureMessage = TempData[FailureMessageKey];

            Semester semester = sid == null
                ? await _semesterService.GetCurrentSemesterAsync()
                : await _semesterService.GetSemesterByIdAsync((int)sid);

            var model = new AddServiceEventAmendmentModel
            {
                Amendment = new ServiceEventAmendment
                {
                    SemesterId = semester.Id
                },
                Semester = semester
            };
            var members = await _memberService.GetRosterForSemesterAsync(semester);
            var memberList = new List<object>();
            foreach (var member in members.OrderBy(m => m.LastName))
            {
                memberList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName +
                        " (" + member.LivingAssignmentForSemester(semester.Id) + ")"
                });
            }
            model.Members = new SelectList(memberList, "UserId", "Name");

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AmendEvents(AddServiceEventAmendmentModel model)
        {
            if (model.Amendment.NumberEvents.Equals(0) ||
                model.Amendment.NumberEvents < -50 ||
                model.Amendment.NumberEvents > 50)
            {
                TempData[FailureMessageKey] = "Please enter a number of events within the range -50 and 50 (excluding 0) in increments of 0.5.";
                return RedirectToAction("AmendEvents", new { sid = model.Amendment.SemesterId });
            }

            await _serviceService.CreateEventAmendmentAsync(model.Amendment);
            TempData[SuccessMessageKey] = "Service amendment added successfully.";

            return RedirectToAction("Index", new { sid = model.Amendment.SemesterId });
        }

        public async Task<ActionResult> UnamendEvents(int aid)
        {
            if (aid <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
            var model = await _serviceService.GetEventAmendmentByIdAsync(aid);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UnamendEvents(ServiceEventAmendment entity)
        {
            if (entity.Id <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);

            var semesterId = entity.SemesterId;
            await _serviceService.DeleteEventAmendmentByIdAsync(entity.Id);

            TempData[SuccessMessageKey] = "Service amendment deleted successfully.";

            return RedirectToAction("Index", new { sid = semesterId });
        }
    }
}