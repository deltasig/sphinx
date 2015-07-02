﻿namespace Dsp.Areas.Service.Controllers
{
    using Entities;
    using global::Dsp.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class HoursController : BaseController
    {
        public async Task<ActionResult> Index(int? s, ServiceHourMessageId? message)
        {
            var thisSemester = await GetThisSemesterAsync();
            if(s == null)
            {
                s = thisSemester.SemesterId;
            }

            switch (message)
            {
                case ServiceHourMessageId.EditFailure:
                case ServiceHourMessageId.DeleteFailure:
                case ServiceHourMessageId.SubmissionFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case ServiceHourMessageId.EditSuccess:
                case ServiceHourMessageId.DeleteSuccess:
                case ServiceHourMessageId.SubmissionSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var semester = await _db.Semesters.FindAsync(s);
            var previousSemester = (await _db.Semesters
                .Where(sem => sem.DateEnd < semester.DateStart)
                .OrderBy(sem => sem.DateEnd)
                .ToListAsync()).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = semester.DateStart
                };

            var model = new ServiceHourIndexModel
            {
                ServiceHours = new List<ServiceHourIndexMemberRowModel>()
            };
            var members = await base.GetRosterForSemester(semester);
            foreach (var m in members)
            {
                var serviceHours = m.ServiceHours
                    .Where(e =>
                        e.Event.DateTimeOccurred > previousSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= semester.DateEnd &&
                        e.Event.IsApproved).ToList();

                var member = new ServiceHourIndexMemberRowModel
                {
                    Member = m,
                    Hours = serviceHours.Sum(h => h.DurationHours),
                    ServiceHours = serviceHours
                };

                model.ServiceHours.Add(member);
            }

            // Identify valid semesters for dropdown
            var events = await _db.Events.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = new List<Semester>();
            foreach (var sem in allSemesters)
            {
                if (events.Any(i => i.DateTimeOccurred >= sem.DateStart && i.DateTimeOccurred <= sem.DateEnd))
                {
                    semesters.Add(sem);
                }
            }
            // Sometimes the current semester doesn't contain any signups, yet we still want it in the list
            if (semesters.All(sem => sem.SemesterId != thisSemester.SemesterId))
            {
                semesters.Add(thisSemester);
            }

            model.SemesterList = await GetCustomSemesterListAsync(semesters);
            model.Semester = semester;

            return View(model);
        }

        public async Task<ActionResult> Submit(int? s, ServiceHourMessageId? message)
        {
            switch (message)
            {
                case ServiceHourMessageId.EditFailure:
                case ServiceHourMessageId.DeleteFailure:
                case ServiceHourMessageId.SubmissionFailure:
                case ServiceHourMessageId.SubmissionFailureTooLow:
                case ServiceHourMessageId.SubmissionFailureTooHigh:
                case ServiceHourMessageId.SubmissionFailureFutureEvent:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case ServiceHourMessageId.EditSuccess:
                case ServiceHourMessageId.DeleteSuccess:
                case ServiceHourMessageId.SubmissionSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var model = new ServiceHourSubmissionModel();
            if (s != null)
            {
                model.Semester = await _db.Semesters.FindAsync(s);
                model.Events = await base.GetAllEventIdsAsEventNameAsync((int)s);
            }
            else
            {
                model.Semester = await base.GetThisSemesterAsync();
                model.Events = await base.GetAllEventIdsAsEventNameAsync();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(ServiceHourSubmissionModel model)
        {
            var selectedEvent = await _db.Events.FindAsync(model.SelectedEventId);
            var semester = await _db.Semesters
                .Where(s => selectedEvent.DateTimeOccurred <= s.DateEnd)
                .OrderBy(s => s.DateStart)
                .FirstAsync();
            // Invalid value
            if (model.HoursServed < 0)
            {
                return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.SubmissionFailureTooLow });
            }

            var fraction = (model.HoursServed % 1) * 10;
            if(!fraction.Equals(5))
            {
                model.HoursServed = Math.Floor(model.HoursServed);
            }

            // Check if event is in the future
            if (selectedEvent.DateTimeOccurred.AddHours(selectedEvent.DurationHours) > DateTime.UtcNow)
            {
                return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.SubmissionFailureFutureEvent });
            }

            // Check if hours submitted is more than held for event
            if (model.HoursServed > selectedEvent.DurationHours)
            {
                return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.SubmissionFailureTooHigh });
            }

            var userId = User.Identity.GetUserId<int>();
            // Check if submission has already been created
            var duplicateSubmission = await _db.ServiceHours.Where(e => (e.EventId == model.SelectedEventId && e.UserId == userId)).ToListAsync();
            if (duplicateSubmission.Any())
            {
                // Delete existing record
                if (model.HoursServed.Equals(0))
                {
                    _db.ServiceHours.Remove(duplicateSubmission.Single());
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.DeleteSuccess });
                }

                // Update submission
                duplicateSubmission.First().DurationHours = model.HoursServed;
                await _db.SaveChangesAsync();
                return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.EditSuccess });
            }

            // No existing, invalid value
            if (model.HoursServed.Equals(0))
            {
                return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.SubmissionFailureTooLow });
            }

            // If no previous submission, create new entry and add it to database
            var submission = new ServiceHour
            {
                UserId = userId,
                DateTimeSubmitted = DateTime.UtcNow,
                EventId = model.SelectedEventId,
                DurationHours = model.HoursServed
            };

            _db.ServiceHours.Add(submission);
            await _db.SaveChangesAsync();
            return RedirectToAction("Submit", new { s = semester.SemesterId, message = ServiceHourMessageId.SubmissionSuccess });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(int? eid, int? uid)
        {
            if (eid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (uid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = await _db.ServiceHours.SingleAsync(s => s.EventId == eid && s.UserId == uid);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred)).SemesterId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(ServiceHour model)
        {
            if (!ModelState.IsValid) return View(model);

            var serviceHour = await _db.ServiceHours.SingleAsync(s => s.EventId == model.EventId && s.UserId == model.UserId);
            serviceHour.DurationHours = model.DurationHours;

            _db.Entry(serviceHour).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(serviceHour.Event.DateTimeOccurred)).SemesterId,
                message = ServiceHourMessageId.EditSuccess
            });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(int? eid, int? uid)
        {
            if (eid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (uid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = await _db.ServiceHours.SingleAsync(s => s.EventId == eid && s.UserId == uid);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred)).SemesterId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(ServiceHour model)
        {
            var serviceHour = await _db.ServiceHours.SingleAsync(s => s.EventId == model.EventId && s.UserId == model.UserId);
            var time = serviceHour.Event.DateTimeOccurred;
            _db.ServiceHours.Remove(serviceHour);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new
            {
                s = (await GetSemestersForUtcDateAsync(time)).SemesterId,
                message = ServiceHourMessageId.DeleteSuccess
            });
        }

        public async Task<ActionResult> Download(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var semester = await _db.Semesters.FindAsync(id);
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < semester.DateStart)
                .OrderBy(s => s.DateEnd)
                .ToListAsync()).LastOrDefault() ?? new Semester
                {
                    DateEnd = semester.DateStart
                };

            var members = await UserManager.Users
                .Where(d =>
                    d.LastName != "Hirtz" &&
                    (d.MemberStatus.StatusName == "Alumnus" ||
                        d.MemberStatus.StatusName == "Active" ||
                        d.MemberStatus.StatusName == "Pledge" ||
                        d.MemberStatus.StatusName == "Neophyte") &&
                    d.PledgeClass.Semester.DateStart <= semester.DateStart &&
                    d.GraduationSemester.DateEnd >= semester.DateEnd)
                .OrderBy(m => m.LastName)
                .ToListAsync();

            var serviceHours = new List<ServiceHour>();
            foreach (var m in members)
            {
                var hours = m.ServiceHours
                    .Where(e =>
                        e.Event.DateTimeOccurred > previousSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= semester.DateEnd &&
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

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-service-" + semester + ".csv");
        }

        public static dynamic GetResultMessage(ServiceHourMessageId? message)
        {
            return message == ServiceHourMessageId.EditFailure ? "Service Hour submission could not be updated for an unknown reason, please contact your administrator."
                : message == ServiceHourMessageId.DeleteFailure ? "Service Hour submission could not be deleted for an unknown reason, please contact your administrator."
                : message == ServiceHourMessageId.SubmissionFailure ? "Service Hour submission failed for an unknown reason, please contact your administrator."
                : message == ServiceHourMessageId.SubmissionFailureTooLow ? "Please enter an amount of hours greater then or equal to 0 in increments of 0.5."
                : message == ServiceHourMessageId.SubmissionFailureTooHigh ? "Please enter an amount of hours less than or equal to the duration of the event in increments of 0.5."
                : message == ServiceHourMessageId.SubmissionFailureFutureEvent ? "You can't submit hours for an event that has not yet occurred."
                : message == ServiceHourMessageId.EditSuccess ? "Service Hour submission was updated successfully."
                : message == ServiceHourMessageId.DeleteSuccess ? "Service Hour submission was deleted successfully."
                : message == ServiceHourMessageId.SubmissionSuccess ? "Service Hour submission was successful."
                : "";
        }

        public enum ServiceHourMessageId
        {
            EditFailure,
            EditSuccess,
            DeleteFailure,
            DeleteSuccess,
            SubmissionFailure,
            SubmissionFailureTooLow,
            SubmissionFailureTooHigh,
            SubmissionFailureFutureEvent,
            SubmissionSuccess
        }
    }
}