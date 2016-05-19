namespace Dsp.Web.Areas.Edu.Controllers
{
    using System;
    using System.Collections.Generic;
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.AspNet.Identity;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class StudyController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];
            
            var thisSemester = await GetThisSemesterAsync();
            var model = new StudyIndexModel
            {
                Periods = await _db.StudyPeriods
                    .Where(p => thisSemester.DateStart <= p.BeginsOn && p.BeginsOn <= thisSemester.DateEnd)
                    .ToListAsync(),
                Semester = thisSemester
            };

            var nowCstAdjusted = ConvertUtcToCst(DateTime.UtcNow).AddMinutes(-60);
            var futureStudySessions = await _db.StudySessions
                .Where(s => s.EndsOn >= nowCstAdjusted)
                .OrderByDescending(s => s.BeginsOn)
                .ToListAsync();
            model.Sessions = futureStudySessions;

            return View(model);
        }

        public async Task<ActionResult> Period(int? id, double da = 2)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var studyPeriod = await _db.StudyPeriods.FindAsync(id);
            if (studyPeriod == null) return HttpNotFound();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var semester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(studyPeriod.BeginsOn));
            var studySessions = await _db.StudySessions
                .Where(s => s.BeginsOn >= studyPeriod.BeginsOn && s.BeginsOn <= studyPeriod.EndsOn)
                .OrderByDescending(s => s.BeginsOn)
                .ToListAsync();

            var model = new StudyPeriodModel
            {
                StudyPeriod = studyPeriod,
                DefaultHourAmount = da,
                Members = await GetRosterForSemester(semester),
                StudySessions = studySessions,
                Semester = semester
            };

            // Check for a previous period
            var previousPeriod = await _db.StudyPeriods
                .Where(p => p.BeginsOn < studyPeriod.BeginsOn)
                .OrderByDescending(p => p.BeginsOn)
                .FirstOrDefaultAsync();
            if (previousPeriod != null)
            {
                var previousPeriodSemester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(previousPeriod.BeginsOn));
                var selectedPeriodSemester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(studyPeriod.BeginsOn));
                if (selectedPeriodSemester == previousPeriodSemester)
                {
                    model.PreviousStudyPeriodId = previousPeriod.Id;
                }
            }

            return View(model);
        }
        
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Report(int? sid)
        {
            var semester = await base.GetThisSemesterAsync();
            if (sid != null)
            {
                semester = await _db.Semesters.FindAsync(sid);
            }

            var model = new StudyReportModel(semester);
            var studyPeriods = await _db.StudyPeriods
                .Where(p => p.BeginsOn >= semester.DateStart && p.EndsOn <= semester.DateEnd)
                .ToListAsync();
            model.Periods = studyPeriods;
            var members = (from p in studyPeriods from a in p.Assignments select a.Member).Distinct();
            foreach(var m in members)
            {
                var record = new StudyReportRecord();
                record.Member = m;
                record.Periods = new List<StudyReportPeriodRecord>();
                foreach(var p in studyPeriods)
                {
                    var periodRecord = new StudyReportPeriodRecord();
                    periodRecord.Period = p;
                    periodRecord.Assignment = p.Assignments.SingleOrDefault(a => a.MemberId == m.Id);
                    if(periodRecord.Assignment != null)
                    {
                        periodRecord.Goal = periodRecord.Assignment.AmountOfHours;
                        periodRecord.Completed = (double)periodRecord.Assignment.StudyHours.Sum(s => s.DurationMinutes);
                        periodRecord.SessionsAttended = periodRecord.Assignment.StudyHours.Count;
                    }
                    record.Periods.Add(periodRecord);
                }
                model.Records.Add(record);
            }

            // Identify valid semesters for dropdown
            var allStudyPeriods = await _db.StudyPeriods.ToListAsync();
            var allSemesters = await _db.Semesters.ToListAsync();
            var semesters = new List<Semester>();
            foreach (var sem in allSemesters)
            {
                if (allStudyPeriods.Any(i => i.BeginsOn >= sem.DateStart && i.EndsOn <= sem.DateEnd))
                {
                    semesters.Add(sem);
                }
            }
            model.SemesterList = GetCustomSemesterListAsync(semesters);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> AssignMemberToPeriod(int mid, int pid, double amount)
        {
            if (amount < 1 || amount > 20 || !(amount % 0.5).Equals(0))
            {
                TempData["FailureMessage"] = "You entered "+ amount + " hours, but " +
                                             "the amount of hours you are allowed to assign a member to is limited " +
                                             "to the interval [1, 20] in increments of 0.5.";
                return RedirectToAction("Period", new { id = pid, da = 1.0 });
            }

            var member = await UserManager.FindByIdAsync(mid);

            var existsAlready = await _db.StudyAssignments.AnyAsync(a => a.MemberId == mid && a.PeriodId == pid);
            if (existsAlready)
            {
                TempData["FailureMessage"] = member + " has already been assigned to this period.";
                return RedirectToAction("Period", new { id = pid, da = amount });
            }

            var newAssignment = new StudyAssignment
            {
                PeriodId = pid,
                MemberId = mid,
                AmountOfHours = amount
            };

            _db.StudyAssignments.Add(newAssignment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = member + " was successfully assigned to study " + amount + " hours during this time period.";
            return RedirectToAction("Period", new { id = pid, da = amount });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> UnassignMemberToPeriod(int aid)
        {
            var assignment = await _db.StudyAssignments.FindAsync(aid);
            var pid = assignment.PeriodId;
            var member = assignment.Member.ToString();
            _db.StudyAssignments.Remove(assignment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = member + " was removed from this study period successfully.";
            return RedirectToAction("Period", new { id = pid });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> ImportPreviousPeriodAssignments(int pid, int ppid)
        {
            var selectedPeriod = await _db.StudyPeriods.FindAsync(pid);
            var previousPeriod = await _db.StudyPeriods.FindAsync(ppid);

            var membersInSelectedPeriod = selectedPeriod.Assignments.Select(a => a.MemberId).ToList();
            var assignmentsAdded = 0;
            foreach (var a in previousPeriod.Assignments.ToList())
            {
                if (membersInSelectedPeriod.Contains(a.MemberId)) continue;

                var newAssignment = new StudyAssignment
                {
                    MemberId = a.MemberId,
                    PeriodId = pid,
                    AmountOfHours = a.AmountOfHours
                };
                _db.StudyAssignments.Add(newAssignment);
                await _db.SaveChangesAsync();
                assignmentsAdded++;
            }

            if (assignmentsAdded == 1)
            {
                TempData["SuccessMessage"] = assignmentsAdded + " assignment was imported to this study period successfully.";
            }
            else if (assignmentsAdded > 1)
            {
                TempData["SuccessMessage"] = assignmentsAdded + " assignments were imported to this study period successfully.";
            }
            else
            {
                TempData["FailureMessage"] = "No assignments were imported to this study period because none were found or they were already imported.";
            }

            return RedirectToAction("Period", new { id = pid });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult CreatePeriod()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> CreatePeriod(StudyPeriod model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.StudyPeriods.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study period was added successfully.";
            return RedirectToAction("CreatePeriod");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditPeriod(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudyPeriods.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditPeriod(StudyPeriod model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study period was updated successfully.";
            return RedirectToAction("Period", new { id = model.Id });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeletePeriod(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudyPeriods.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("DeletePeriod"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeletePeriodConfirmed(int id)
        {
            var model = await _db.StudyPeriods.FindAsync(id);
            _db.StudyPeriods.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study period was deleted successfully.";
            return RedirectToAction("Index");
        }
        
        public async Task<ActionResult> Sessions(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var studySession = await _db.StudySessions.FindAsync(id);
            if (studySession == null) return HttpNotFound();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var semester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(studySession.BeginsOn));

            var model = new StudySessionModel
            {
                StudySession = studySession,
                Members = await GetRosterForSemester(semester),
                StudyPeriod = await _db.StudyPeriods
                    .SingleOrDefaultAsync(p => p.BeginsOn <= studySession.BeginsOn && studySession.BeginsOn <= p.EndsOn)
            };
           
            // Grab any other sessions going on during the same study period
            model.StudySessions = await _db.StudySessions
                .Where(s => model.StudyPeriod.BeginsOn <= s.BeginsOn && s.BeginsOn <= model.StudyPeriod.EndsOn)
                .ToListAsync();

            // Check for a previous period
            var previousSession = await _db.StudySessions
                .Where(p => p.BeginsOn < studySession.BeginsOn)
                .OrderByDescending(p => p.BeginsOn)
                .FirstOrDefaultAsync();
            if (previousSession != null)
            {
                var previousSessionSemester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(previousSession.BeginsOn));
                var selectedSessionSemester = await GetSemestersForUtcDateAsync(ConvertCstToUtc(studySession.BeginsOn));
                if (selectedSessionSemester == previousSessionSemester)
                {
                    model.PreviousStudySessionId = previousSession.Id;
                }
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> ImportPreviousSessions(int pid, int ppid)
        {
            var selectedPeriod = await _db.StudyPeriods.FindAsync(pid);
            var previousPeriod = await _db.StudyPeriods.FindAsync(ppid);
            var sessionsAdded = 0;

            // We must check that the previous study period begins on the same day of the week.
            // Otherwise, it becomes subjective how we translate its study sessions to the selected period.
            if (selectedPeriod.BeginsOn.DayOfWeek == previousPeriod.BeginsOn.DayOfWeek)
            {
                var sessionsInPreviousPeriod = await GetSessionsForPeriod(previousPeriod);

                foreach (var s in sessionsInPreviousPeriod)
                {
                    var secsFromPeriodStartToSessionStart = (s.BeginsOn - previousPeriod.BeginsOn).TotalSeconds;
                    var secsFromPeriodStartToSessionEnd = (s.EndsOn - previousPeriod.BeginsOn).TotalSeconds;

                    var newSession = new StudySession
                    {
                        BeginsOn = selectedPeriod.BeginsOn.AddSeconds(secsFromPeriodStartToSessionStart),
                        EndsOn = selectedPeriod.BeginsOn.AddSeconds(secsFromPeriodStartToSessionEnd),
                        Location = s.Location
                    };
                    _db.StudySessions.Add(newSession);
                    await _db.SaveChangesAsync();
                    sessionsAdded++;

                    foreach (var p in s.Proctors)
                    {
                        var newProctor = new StudyProctor
                        {
                            MemberId = p.MemberId,
                            SessionId = newSession.Id
                        };
                        _db.StudyProctors.Add(newProctor);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            if (sessionsAdded == 1)
            {
                TempData["SuccessMessage"] = sessionsAdded + " session was imported to this study period.";
            }
            else if (sessionsAdded > 1)
            {
                TempData["SuccessMessage"] = sessionsAdded + " sessions were imported to this study period.";
            }
            else
            {
                TempData["FailureMessage"] = "No sessions were imported to this study period because none were found or " +
                                             "because the previous study period begins on a different day of the week.";
            }

            return RedirectToAction("Period", new { id = pid });
        }

        private async Task<List<StudySession>> GetSessionsForPeriod(StudyPeriod period)
        {
            var sessions = await _db.StudySessions
                .Where(s => s.BeginsOn >= period.BeginsOn && s.BeginsOn <= period.EndsOn)
                .ToListAsync();
            return sessions;
        }
        private async Task<StudyPeriod> GetPeriodForSession(StudySession s)
        {
            var period = await _db.StudyPeriods
                .SingleOrDefaultAsync(p => p.BeginsOn <= s.BeginsOn && p.EndsOn >= s.BeginsOn);
            return period;
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> AssignProctor(int mid, int sid)
        {
            var member = await UserManager.FindByIdAsync(mid);
            var session = await _db.StudySessions.FindAsync(sid);
            var newProctor = new StudyProctor
            {
                MemberId = mid,
                SessionId = sid
            };

            _db.StudyProctors.Add(newProctor);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = member + " was successfully added as proctor to study session " +
                                         "occurring on " + session.BeginsOn.ToShortDateString() +".";
            return RedirectToAction("Sessions", new { id = sid });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> UnassignProctor(int pid)
        {
            var proctor = await _db.StudyProctors.FindAsync(pid);
            var sid = proctor.SessionId;
            var member = proctor.Member.ToString();
            _db.StudyProctors.Remove(proctor);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = member + " was removed from being a proctor successfully.";
            return RedirectToAction("Sessions", new { id = sid });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public ActionResult CreateSession()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> CreateSession(StudySession model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.StudySessions.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study session was added successfully.";
            return RedirectToAction("CreateSession");
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditSession(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudySessions.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditSession(StudySession model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study session was updated successfully.";
            return RedirectToAction("Sessions", new { id = model.Id });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteSession(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudySessions.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("DeleteSession"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteSessionConfirmed(int id)
        {
            var model = await _db.StudySessions.FindAsync(id);
            var period = await GetPeriodForSession(model);
            _db.StudySessions.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study session was deleted successfully.";
            if(period == null) return RedirectToAction("Index");
            return RedirectToAction("Period", new {id = period.Id});
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SessionSignIn(int aid, int sid, int mid)
        {
            var assignment = await _db.StudyAssignments.FindAsync(aid);
            var session = await _db.StudySessions.FindAsync(sid);
            var member = await UserManager.FindByIdAsync(mid);
            if (assignment == null || session == null || member == null)
                return HttpNotFound();
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studyHour = await _db.StudyHours
                .SingleOrDefaultAsync(s => s.AssignmentId == aid && s.SessionId == sid && s.MemberId == mid);
            if (studyHour != null)
            {
                TempData["FailureMessage"] = "Member has already been signed in.";
                return RedirectToAction("Sessions", new { id = sid });
            }

            var model = new StudyHour
            {
                MemberId = mid,
                SessionId = sid,
                AssignmentId = aid,
                SignedInOn = ConvertUtcToCst(DateTime.UtcNow)
            };

            _db.StudyHours.Add(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Member was signed in.";
            return RedirectToAction("Sessions", new { id = sid });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> SessionSignOut(int shid)
        {
            var studyHour = await _db.StudyHours.FindAsync(shid);
            if (studyHour == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !studyHour.Session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (studyHour.SignedOutOn != null || studyHour.DurationMinutes != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nowCst = ConvertUtcToCst(DateTime.UtcNow);
            if (nowCst > studyHour.Session.EndsOn.AddMinutes(15))
            {
                studyHour.SignedOutOn = studyHour.Session.EndsOn.AddMinutes(15);
            }
            else
            {
                studyHour.SignedOutOn = nowCst;
            }
            studyHour.DurationMinutes = Math.Round(((DateTime)studyHour.SignedOutOn - studyHour.SignedInOn).TotalMinutes, 0);

            _db.Entry(studyHour).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Member is signed out.";
            return RedirectToAction("Sessions", new { id = studyHour.SessionId });
        }
        
        public async Task<ActionResult> EditSignIn(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudyHours.FindAsync(id);
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !model.Session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSignIn(StudyHour model)
        {
            if (!ModelState.IsValid) return View(model);
            var assignment = await _db.StudyAssignments.FindAsync(model.AssignmentId);
            var session = await _db.StudySessions.FindAsync(model.SessionId);
            var member = await UserManager.FindByIdAsync(model.MemberId);
            if (assignment == null || session == null || member == null || model.SignedOutOn == null)
                return HttpNotFound();
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (model.SignedInOn < session.BeginsOn.AddMinutes(-15))
            {
                ViewBag.FailureMessage = "The sign in time cannot exceed 15 minutes before the session ends.  " +
                                         "Please correct the sign in time and try again";
                return View(model);
            }
            if (model.SignedInOn >= model.SignedOutOn)
            {
                ViewBag.FailureMessage = "You can't sign out before you signed in.  " +
                                         "Please correct the sign out time and try again.";
                return View(model);
            }
            if (model.SignedOutOn > session.EndsOn.AddMinutes(15))
            {
                ViewBag.FailureMessage = "The sign out time cannot exceed 15 minutes after the session ends.  " +
                                         "Please correct the sign out time and try again";
                return View(model);
            }
            model.DurationMinutes = Math.Round(((DateTime)model.SignedOutOn - model.SignedInOn).TotalMinutes, 0);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study hours were updated successfully.";
            return RedirectToAction("Sessions", new { id = session.Id });
        }

        public async Task<ActionResult> DeleteSignIn(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.StudyHours.FindAsync(id);
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !model.Session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("DeleteSignIn"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteSignInConfirmed(int id)
        {
            var model = await _db.StudyHours.FindAsync(id);
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !model.Session.Proctors.Select(p => p.MemberId).Contains(User.Identity.GetUserId<int>()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sessionId = model.SessionId;
            _db.StudyHours.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study hours were deleted successfully.";
            return RedirectToAction("Sessions", new { id = sessionId });
        }
    }
}
