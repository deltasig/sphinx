namespace Dsp.Web.Areas.Service.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
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

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator")]
    public class HoursController : BaseController
    {
        public async Task<ActionResult> Index(int? s)
        {
            var thisSemester = await GetThisSemesterAsync();
            if (s == null)
            {
                s = thisSemester.SemesterId;
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

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
            var members = await GetRosterForSemester(semester);
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

            model.SemesterList = await GetSemesterList(thisSemester);
            model.Semester = semester;

            return View(model);
        }

        public async Task<ActionResult> Submit(int? s)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var model = new ServiceHourSubmissionModel();
            if (s != null)
            {
                model.Semester = await _db.Semesters.FindAsync(s);
                model.Events = await GetAllEventIdsAsEventNameAsync((int)s);
            }
            else
            {
                model.Semester = await GetThisSemesterAsync();
                model.Events = await GetAllEventIdsAsEventNameAsync();
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(ServiceHourSubmissionModel model)
        {
            var selectedEvent = await _db.ServiceEvents.FindAsync(model.SelectedEventId);
            var semester = await _db.Semesters
                .Where(s => selectedEvent.DateTimeOccurred <= s.DateEnd)
                .OrderBy(s => s.DateStart)
                .FirstAsync();
            // Invalid value
            if (model.HoursServed < 0)
            {
                ViewBag.FailureMessage = "Please enter an amount of hours greater than or equal to 0 in increments of 0.5.";
                model.Semester = semester;
                model.Events = await GetAllEventIdsAsEventNameAsync(semester.SemesterId);
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
                model.Semester = semester;
                model.Events = await GetAllEventIdsAsEventNameAsync(semester.SemesterId);
                return View(model);
            }

            // Check if hours submitted are more than the duration of the event
            if (model.HoursServed > selectedEvent.DurationHours)
            {
                ViewBag.FailureMessage = "Please enter an amount of hours less than or equal to the duration of the event in increments of 0.5.";
                model.Semester = semester;
                model.Events = await GetAllEventIdsAsEventNameAsync(semester.SemesterId);
                return View(model);
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
                    TempData["SuccessMessage"] = "Service hours deleted successfully.";

                    return RedirectToAction("Submit", new { s = semester.SemesterId });
                }

                // Update submission
                duplicateSubmission.First().DurationHours = model.HoursServed;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service hours updated successfully.";

                return RedirectToAction("Submit", new { s = semester.SemesterId });
            }

            // No existing, invalid value
            if (model.HoursServed.Equals(0))
            {
                ViewBag.FailureMessage = "Please enter an amount of hours greater than 0 in increments of 0.5.";
                model.Semester = semester;
                model.Events = await GetAllEventIdsAsEventNameAsync(semester.SemesterId);
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

            _db.ServiceHours.Add(submission);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service hours submitted successfully.";

            return RedirectToAction("Submit", new { s = semester.SemesterId });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(int? eid, int? uid)
        {
            if (eid == null || uid == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.ServiceHours.SingleAsync(s => s.EventId == eid && s.UserId == uid);
            if (model == null) return HttpNotFound();

            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred)).SemesterId;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Edit(ServiceHour model)
        {
            if (!ModelState.IsValid) return View(model);

            var serviceHour = await _db.ServiceHours.SingleAsync(s => s.EventId == model.EventId && s.UserId == model.UserId);
            serviceHour.DurationHours = model.DurationHours;

            _db.Entry(serviceHour).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service hours updated successfully.";

            var semesterId = (await GetSemestersForUtcDateAsync(serviceHour.Event.DateTimeOccurred)).SemesterId;

            return RedirectToAction("Index", new { s = semesterId });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(int? eid, int? uid)
        {
            if (eid == null || uid == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.ServiceHours.SingleAsync(s => s.EventId == eid && s.UserId == uid);
            if (model == null) return HttpNotFound();

            ViewBag.SemesterId = (await GetSemestersForUtcDateAsync(model.Event.DateTimeOccurred)).SemesterId;

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Delete(ServiceHour model)
        {
            var serviceHour = await _db.ServiceHours.SingleAsync(s => s.EventId == model.EventId && s.UserId == model.UserId);
            var time = serviceHour.Event.DateTimeOccurred;

            _db.ServiceHours.Remove(serviceHour);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service hours deleted successfully.";

            var semesterId = (await GetSemestersForUtcDateAsync(time)).SemesterId;

            return RedirectToAction("Index", new { s = semesterId });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> Amendments(int? s)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var thisSemester = await GetThisSemesterAsync();
            if (s == null)
            {
                s = thisSemester.SemesterId;
            }
            var semester = await _db.Semesters.FindAsync(s);

            var model = new ServiceAmendmentModel
            {
                Semester = semester,
                SemesterList = await GetSemesterList(thisSemester),
                ServiceHourAmendments = await _db.ServiceHourAmendments.Where(a => a.SemesterId == s).ToListAsync(),
                ServiceEventAmendments = await _db.ServiceEventAmendments.Where(a => a.SemesterId == s).ToListAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> AddHourAmendment(int? s)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var thisSemester = await GetThisSemesterAsync();
            if (s == null) s = thisSemester.SemesterId;
            var semester = await _db.Semesters.FindAsync(s);
            var model = new ServiceAddHourAmendmentModel
            {
                Amendment = new ServiceHourAmendment
                {
                    SemesterId = semester.SemesterId
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
                        " (" + member.LivingAssignmentForSemester(semester.SemesterId) + ")"
                });
            }
            model.Members = new SelectList(memberList, "UserId", "Name");

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> AddHourAmendment(ServiceAddHourAmendmentModel model)
        {
            if (model.Amendment.AmountHours.Equals(0) ||
                model.Amendment.AmountHours < -50 ||
                model.Amendment.AmountHours > 50)
            {
                TempData["FailureMessage"] = "Please enter hours within the range -50 and 50 (excluding 0) in increments of 0.5.";
                return RedirectToAction("AddHourAmendment", new { s = model.Amendment.SemesterId });
            }
            // Adjust hours to nearest half hour.
            var fraction = (model.Amendment.AmountHours % 1) * 10;
            if (!fraction.Equals(5))
            {
                model.Amendment.AmountHours = Math.Floor(model.Amendment.AmountHours);
            }

            _db.ServiceHourAmendments.Add(model.Amendment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service amendment added successfully.";

            return RedirectToAction("AddHourAmendment", new { s = model.Amendment.SemesterId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> DeleteHourAmendment(int? aid)
        {
            if (aid == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var amendment = await _db.ServiceHourAmendments.FindAsync(aid);
            var semesterId = amendment.SemesterId;

            _db.ServiceHourAmendments.Remove(amendment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service amendment deleted successfully.";

            return RedirectToAction("Amendments", new { s = semesterId });
        }

        [Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> AddEventAmendment(int? s)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var thisSemester = await GetThisSemesterAsync();
            if (s == null) s = thisSemester.SemesterId;
            var semester = await _db.Semesters.FindAsync(s);
            var model = new ServiceAddEventAmendmentModel
            {
                Amendment = new ServiceEventAmendment
                {
                    SemesterId = semester.SemesterId
                },
                Semester = semester
            };
            var members = await GetRosterForSemester(semester);
            var memberList = new List<object>();
            foreach (var member in members.OrderBy(m => m.LastName))
            {
                memberList.Add(new
                {
                    UserId = member.Id,
                    Name = member.FirstName + " " + member.LastName +
                        " (" + member.LivingAssignmentForSemester(semester.SemesterId) + ")"
                });
            }
            model.Members = new SelectList(memberList, "UserId", "Name");

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> AddEventAmendment(ServiceAddEventAmendmentModel model)
        {
            if (model.Amendment.NumberEvents.Equals(0) ||
                model.Amendment.NumberEvents < -50 ||
                model.Amendment.NumberEvents > 50)
            {
                TempData["FailureMessage"] = "Please enter a number of events within the range -50 and 50 (excluding 0) in increments of 0.5.";
                return RedirectToAction("AddEventAmendment", new { s = model.Amendment.SemesterId });
            }

            _db.ServiceEventAmendments.Add(model.Amendment);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Service amendment added successfully.";

            return RedirectToAction("AddEventAmendment", new { s = model.Amendment.SemesterId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Service")]
        public async Task<ActionResult> DeleteEventAmendment(int? aid)
        {
            if (aid == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var amendment = await _db.ServiceEventAmendments.FindAsync(aid);
            var semesterId = amendment.SemesterId;

            _db.ServiceEventAmendments.Remove(amendment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service amendment deleted successfully.";

            return RedirectToAction("Amendments", new { s = semesterId });
        }

        private async Task<SelectList> GetSemesterList(Semester thisSemester)
        {
            // Identify valid semesters for dropdown
            var events = await _db.ServiceEvents.ToListAsync();
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

            return GetCustomSemesterListAsync(semesters);
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
                        d.MemberStatus.StatusName == "New" ||
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
    }
}