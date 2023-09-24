namespace Dsp.WebCore.Areas.Service.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

[Authorize]
public class HoursController : BaseController
{
    private readonly IMemberService _memberService;
    private readonly IPositionService _positionService;
    private readonly ISemesterService _semesterService;
    private readonly IServiceService _serviceService;

    public HoursController(IMemberService memberService, IPositionService positionService, ISemesterService semesterService, IServiceService serviceService)
    {
        _memberService = memberService;
        _positionService = positionService;
        _semesterService = semesterService;
        _serviceService = serviceService;
    }

    public async Task<ActionResult> Index(int? sid)
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        Semester selectedSemester = sid == null
            ? currentSemester
            : await _semesterService.GetSemesterByIdAsync((int)sid);

        var rosterProgress = await _serviceService.GetRosterProgressBySemesterIdAsync(selectedSemester.Id);
        var semestersWithEvents = await _serviceService.GetSemestersWithEventsAsync(currentSemester);
        var semesterList = GetSemesterSelectList(semestersWithEvents);
        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Service");
        var navModel = new ServiceNavModel(hasElevatedPermissions, selectedSemester, semesterList);
        var model = new ServiceHourIndexModel(navModel, rosterProgress);

        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];
        return View(model);
    }

    public async Task<ActionResult> Submit(int? sid)
    {
        var model = new ServiceHourSubmissionModel();
        if (sid != null)
        {
            model.Semester = await _semesterService.GetSemesterByIdAsync((int)sid);
            var events = await _serviceService.GetEventsForSemesterAsync(model.Semester);
            model.Events = GetServiceEventSelectList(events);
        }
        else
        {
            model.Semester = await GetThisSemesterAsync();
            var events = await _serviceService.GetEventsForSemesterAsync(model.Semester);
            model.Events = GetServiceEventSelectList(events);
        }

        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Submit(ServiceHourSubmissionModel model)
    {
        var selectedEvent = await _serviceService.GetEventByIdAsync(model.SelectedEventId);
        // Invalid value
        if (model.HoursServed < 0)
        {
            ViewBag.FailureMessage = "Please enter an amount of hours greater than or equal to 0 in increments of 0.5.";
            model.Semester = selectedEvent.Semester;
            var events = await _serviceService.GetEventsForSemesterAsync(selectedEvent.Semester);
            model.Events = GetServiceEventSelectList(events);
            return View(model);
        }

        var fraction = (model.HoursServed % 1) * 10;
        if (!fraction.Equals(5))
        {
            model.HoursServed = Math.Floor(model.HoursServed);
        }

        // Check if event is in the future
        if (selectedEvent.DateTimeOccurred.AddHours(selectedEvent.DurationHours) > DateTime.UtcNow)
        {
            ViewBag.FailureMessage = "You can't submit hours for an event that has not yet occurred.";
            model.Semester = selectedEvent.Semester;
            var events = await _serviceService.GetEventsForSemesterAsync(selectedEvent.Semester);
            model.Events = GetServiceEventSelectList(events);
            return View(model);
        }

        // Check if hours submitted are more than the duration of the event
        if (model.HoursServed > selectedEvent.DurationHours)
        {
            ViewBag.FailureMessage = "Please enter an amount of hours less than or equal to the duration of the event in increments of 0.5.";
            model.Semester = selectedEvent.Semester;
            var events = await _serviceService.GetEventsForSemesterAsync(selectedEvent.Semester);
            model.Events = GetServiceEventSelectList(events);
            return View(model);
        }

        var userId = User.GetUserId();
        // Check if submission has already been created
        var duplicateSubmission = await _serviceService.GetHoursAsync(model.SelectedEventId, userId);
        if (duplicateSubmission != null)
        {
            // Delete existing record
            if (model.HoursServed.Equals(0))
            {
                await _serviceService.DeleteHoursAsync(duplicateSubmission);
                TempData[SuccessMessageKey] = "Service hours deleted successfully.";

                return RedirectToAction("Submit", new { sid = selectedEvent.SemesterId });
            }

            // Update submission
            duplicateSubmission.DurationHours = model.HoursServed;
            await _serviceService.UpdateHoursAsync(duplicateSubmission);
            TempData[SuccessMessageKey] = "Service hours updated successfully.";

            return RedirectToAction("Submit", new { sid = selectedEvent.SemesterId });
        }

        // No existing, invalid value
        if (model.HoursServed.Equals(0))
        {
            ViewBag.FailureMessage = "Please enter an amount of hours greater than 0 in increments of 0.5.";
            model.Semester = selectedEvent.Semester;
            var events = await _serviceService.GetEventsForSemesterAsync(selectedEvent.Semester);
            model.Events = GetServiceEventSelectList(events);
            return View(model);
        }

        // If no previous submission, create new entry and add it to database
        var submission = new ServiceHour
        {
            UserId = userId,
            DateTimeSubmitted = DateTime.UtcNow,
            EventId = model.SelectedEventId,
            DurationHours = model.HoursServed
        };

        await _serviceService.CreateHoursAsync(submission);
        TempData[SuccessMessageKey] = "Service hours submitted successfully.";

        return RedirectToAction("Submit", new { sid = selectedEvent.SemesterId });
    }

    [Authorize]
    public async Task<ActionResult> Edit(int eid, int uid)
    {
        if (eid <= 0 || uid <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await _serviceService.GetHoursAsync(eid, uid);
        if (model == null) return NotFound();

        ViewBag.SemesterId = model.Event.SemesterId;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> Edit(ServiceHour model)
    {
        if (!ModelState.IsValid) return View(model);

        var entity = await _serviceService.GetHoursAsync(model.EventId, model.UserId);
        entity.DurationHours = model.DurationHours;

        await _serviceService.UpdateHoursAsync(entity);
        TempData[SuccessMessageKey] = "Service hours updated successfully.";

        var serviceEvent = await _serviceService.GetEventByIdAsync(model.EventId);
        return RedirectToAction("Index", new { sid = serviceEvent.SemesterId });
    }

    [Authorize]
    public async Task<ActionResult> Delete(int eid, int uid)
    {
        if (eid <= 0 || uid <= 0) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await _serviceService.GetHoursAsync(eid, uid);
        if (model == null) return NotFound();

        ViewBag.SemesterId = model.Event.SemesterId;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize]
    public async Task<ActionResult> Delete(ServiceHour model)
    {
        var entity = await _serviceService.GetHoursAsync(model.EventId, model.UserId);
        var time = entity.Event.DateTimeOccurred;

        await _serviceService.DeleteHoursAsync(entity);
        TempData[SuccessMessageKey] = "Service hours deleted successfully.";

        var serviceEvent = await _serviceService.GetEventByIdAsync(model.EventId);
        return RedirectToAction("Index", new { sid = serviceEvent.SemesterId });
    }

    public async Task<ActionResult> Download(int? sid)
    {
        if (sid == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }

        var selectedSemester = await _semesterService.GetSemesterByIdAsync((int)sid);
        var priorSemester = await _semesterService.GetPriorSemesterAsync(selectedSemester);
        var members = await _memberService.GetRosterForSemesterAsync(selectedSemester);

        var serviceHours = new List<ServiceHour>();
        foreach (var m in members)
        {
            var hours = m.ServiceHours
                .Where(e =>
                    e.Event.DateTimeOccurred > priorSemester.DateEnd &&
                    e.Event.DateTimeOccurred <= selectedSemester.DateEnd &&
                    e.Event.IsApproved);
            serviceHours.AddRange(hours);
        }

        var events = serviceHours
            .Select(h => h.Event)
            .Distinct()
            .OrderBy(e => e.EventName)
            .ToList();

        var header = "Last Name, First Name";
        foreach (var e in events)
        {
            header += "," + e.EventName.Replace(",", ";") + "(" + e.DurationHours + " hrs)";
        }
        header += ",Member Totals";

        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var m in members)
        {
            var line = m.LastName.Replace(",", "") + ", " + m.FirstName.Replace(",", "");
            var hoursForMember = serviceHours
                .Where(h => h.UserId == m.Id)
                .ToList();
            foreach (var e in events)
            {
                line += ",";
                var turnIns = hoursForMember
                    .Where(h => h.EventId == e.EventId).ToList();
                if (turnIns.Any())
                {
                    line += turnIns.Sum(h => h.DurationHours);
                }
                else
                {
                    line += "0";
                }
            }
            line += "," + hoursForMember.Sum(h => h.DurationHours);
            sb.AppendLine(line);
        }
        var totalsLine = ",Event Totals";
        foreach (var e in events)
        {
            totalsLine += "," + e.ServiceHours.Sum(h => h.DurationHours);
        }
        totalsLine += "," + events.Sum(e => e.ServiceHours.Sum(h => h.DurationHours));
        sb.AppendLine(totalsLine);

        return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-service-" + selectedSemester + ".csv");
    }

    private SelectList GetServiceEventSelectList(IEnumerable<ServiceEvent> events)
    {
        var newList = new List<object>();
        foreach (var e in events)
        {
            var utcEventTime = _serviceService.ConvertUtcToCst(e.DateTimeOccurred);
            newList.Add(new
            {
                e.EventId,
                EventName = $"{utcEventTime}: {e.EventName} (Lasted {e.DurationHours} hours)"
            });
        }

        return new SelectList(newList, "EventId", "EventName");
    }
}
