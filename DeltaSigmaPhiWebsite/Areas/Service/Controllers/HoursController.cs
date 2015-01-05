namespace DeltaSigmaPhiWebsite.Areas.Service.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class HoursController : BaseController
    {
        public async Task<ActionResult> Index(ServiceHourIndexFilterModel model)
        {
            if(model.SelectedSemester == null)
            {
                model.SelectedSemester = await GetThisSemestersIdAsync();
            }

            var semester = await _db.Semesters.FindAsync(model.SelectedSemester);
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
                var member = new ServiceHourIndexModel
                {
                    Member = m,
                    Hours = m.ServiceHours
                        .Where(e => 
                            e.Event.DateTimeOccurred > previousSemester.DateEnd && 
                            e.DateTimeSubmitted <= semester.DateEnd)
                        .Sum(h => h.DurationHours),
                    Semester = semester
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
            
            // Check if hours submitted is more than held for event
            var selectedEvent = await _db.Events.SingleAsync(e => e.EventId == model.SelectedEventId);
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
    }
}