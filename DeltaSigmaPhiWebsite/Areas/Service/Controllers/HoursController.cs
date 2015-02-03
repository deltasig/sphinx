namespace DeltaSigmaPhiWebsite.Areas.Service.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class HoursController : BaseController
    {
        public async Task<ActionResult> Index(ServiceHourIndexFilterModel model, ServiceHourMessageId? message)
        {
            if(model.SelectedSemester == null)
            {
                model.SelectedSemester = await GetThisSemestersIdAsync();
            }

            switch (message)
            {
                case ServiceHourMessageId.EditFailure:
                case ServiceHourMessageId.DeleteFailure:
                    ViewBag.FailMessage = GetResultMessage(message);
                    break;
                case ServiceHourMessageId.EditSuccess:
                case ServiceHourMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetResultMessage(message);
                    break;
            }

            var semester = await _db.Semesters.FindAsync(model.SelectedSemester);
            var lastSemester = await base.GetLastSemesterAsync();
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < semester.DateStart)
                .OrderBy(s => s.DateEnd)
                .ToListAsync()).LastOrDefault() ?? new Semester
                {
                    // In case they pick the very first semester in the system.
                    DateEnd = semester.DateStart
                };

            model.ServiceHours = new List<ServiceHourIndexModel>();
            var members = await _db.Members
                .Where(d =>
                    d.LastName != "Hirtz"  &&
                    (d.MemberStatus.StatusName == "Alumnus" || 
                        d.MemberStatus.StatusName == "Active" || 
                        d.MemberStatus.StatusName == "Pledge" || 
                        d.MemberStatus.StatusName == "Neophyte") &&
                    d.PledgeClass.Semester.DateStart <= semester.DateStart &&
                    d.GraduationSemester.DateEnd >= semester.DateEnd)
                .ToListAsync();

            foreach (var m in members)
            {
                var serviceHours = m.ServiceHours
                    .Where(e =>
                        e.Event.DateTimeOccurred > previousSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= semester.DateEnd &&
                        e.Event.IsApproved);

                var member = new ServiceHourIndexModel
                {
                    Member = m,
                    Hours = serviceHours.Sum(h => h.DurationHours),
                    Semester = semester,
                    ServiceHours = serviceHours
                };

                model.ServiceHours.Add(member);
            }

            model.SemesterList = await GetSemesterListAsync();

            return View(model);
        }

        public async Task<ActionResult> Submit()
        {
            var model = new ServiceHourSubmissionModel
            {
                Events = await base.GetAllEventIdsAsEventNameAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(ServiceHourSubmissionModel model)
        {
            string message;
            // Invalid value
            if (model.HoursServed < 0)
            {
                message = "Please enter a number of hours to submit greater than 0.";
                return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
            }

            var fraction = (model.HoursServed % 1) * 10;
            if(!fraction.Equals(5))
            {
                model.HoursServed = Math.Floor(model.HoursServed);
            }

            var userId = WebSecurity.GetUserId(User.Identity.Name);

            var selectedEvent = await _db.Events.SingleAsync(e => e.EventId == model.SelectedEventId);
            // Check if event is in the future
            if (selectedEvent.DateTimeOccurred.AddHours(selectedEvent.DurationHours) > DateTime.UtcNow)
            {
                message = "You cannot submit service hours for an event that has not yet occurred.";
                return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
            }

            // Check if hours submitted is more than held for event
            if (model.HoursServed > selectedEvent.DurationHours)
            {
                message = "Maximum submission for " + selectedEvent.EventName + " is " + selectedEvent.DurationHours + " hours.";
                return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
            }
            
            // Check if submission has already been created
            var duplicateSubmission = await _db.ServiceHours.Where(e => (e.EventId == model.SelectedEventId && e.UserId == userId)).ToListAsync();
            if (duplicateSubmission.Any())
            {
                // Delete existing record
                if (model.HoursServed.Equals(0))
                {
                    _db.ServiceHours.Remove(duplicateSubmission.Single());
                    await _db.SaveChangesAsync();
                    message = "Your submission was deleted.";
                    return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
                }

                // Update submission
                duplicateSubmission.First().DurationHours = model.HoursServed;
                await _db.SaveChangesAsync();
                message = "Your hours for" + selectedEvent.EventName + " event have been updated.";
                return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
            }

            // No existing, invalid value
            if (model.HoursServed.Equals(0))
            {
                message = "Please enter a number of hours to submit greater than 0.";
                return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
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
            message = "Service hours submitted successfully.";
            return RedirectToAction("Index", "Home", new { message, area = "Sphinx" });
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

            ViewBag.Semester = (await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred)).SemesterId;

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
                SelectedSemester = (await GetSemestersForUtcDateAsync(serviceHour.Event.DateTimeOccurred)).SemesterId,
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

            var semester = await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred);
            ViewBag.Semester = semester.SemesterId;

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
                SelectedSemester = (await GetSemestersForUtcDateAsync(time)).SemesterId,
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

            var members = await _db.Members
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
                    .Where(h => h.UserId == m.UserId)
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
                : message == ServiceHourMessageId.EditSuccess ? "Service Hour submission was updated successfully."
                : message == ServiceHourMessageId.DeleteSuccess ? "Service Hour submission was deleted successfully."
                : "";
        }

        public enum ServiceHourMessageId
        {
            EditFailure,
            EditSuccess,
            DeleteFailure,
            DeleteSuccess
        }
    }
}