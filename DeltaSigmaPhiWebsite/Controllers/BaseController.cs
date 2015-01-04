namespace DeltaSigmaPhiWebsite.Controllers
{
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;

    public class BaseController : Controller
    {
        protected readonly DspDbContext _db = new DspDbContext();

        protected virtual async Task<List<Member>> GetAllActiveMembersAsync()
        {
            return await _db.Members.Where(m => m.MemberStatus.StatusName == "Active").ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllPledgeMembersAsync()
        {
            return await _db.Members.Where(m => m.MemberStatus.StatusName == "Pledge").ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllActivePledgeNeophyteMembersAsync()
        {
            return await _db.Members
                .Where(m => m.MemberStatus.StatusName == "Active" ||
                            m.MemberStatus.StatusName == "Pledge" ||
                            m.MemberStatus.StatusName == "Neophyte")
                .ToListAsync();
        }
        protected virtual async Task<IEnumerable<Member>> GetAllAlumniMembersAsync()
        {
            return await _db.Members.Where(m => m.MemberStatus.StatusName == "Alumnus").ToListAsync();
        }
        protected virtual async Task<IEnumerable<Semester>> GetThisAndNextSemesterListAsync()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= now)
                .OrderBy(s => s.DateStart)
                .Take(2)
                .ToListAsync();

            return thisAndComingSemesters;
        }
        protected virtual async Task<SelectList> GetThisAndNextSemesterSelectListAsync()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= now)
                .OrderBy(s => s.DateStart)
                .ToListAsync();

            var newList = new List<object>
            {
                new
                {
                    thisAndComingSemesters[0].SemesterId,
                    Name = thisAndComingSemesters[0].ToString()
                },
                new
                {
                    thisAndComingSemesters[1].SemesterId,
                    Name = thisAndComingSemesters[1].ToString()
                }
            };

            return new SelectList(newList, "SemesterId", "Name");
        }
        protected virtual async Task<Semester> GetThisOrLastSemesterAsync()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            return (await _db.Semesters
                .Where(s => s.DateEnd <= now)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .Last();
        }
        protected virtual async Task<Semester> GetThisSemesterAsync()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            return (await _db.Semesters
                    .Where(s => s.DateEnd >= now)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .First();
        }
        protected virtual async Task<Semester> GetLastSemesterAsync()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow);
            return (await _db.Semesters
                    .Where(s => s.DateEnd < now)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .Last();
        }
        protected virtual async Task<int?> GetThisSemestersIdAsync()
        {
            var semesters = await _db.Semesters.ToListAsync();
            if (semesters.Count <= 0) return null;

            try
            {
                var thisSemester = await GetThisSemesterAsync();
                return thisSemester.SemesterId;
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected virtual async Task<IEnumerable<object>> GetUserIdListAsFullNameWithNoneNonSelectListAsync()
        {
            var members = (await GetAllActivePledgeNeophyteMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return newList;
        }
        protected virtual async Task<IEnumerable<object>> GetAlumniIdListAsFullNameWithNoneNonSelectListAsync()
        {
            var members = (await GetAllAlumniMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return newList;
        }
        protected virtual async Task<SelectList> GetSemesterListAsync()
        {
            var semesters = await _db.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
            var newList = new List<object>();

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name", (await GetThisSemesterAsync()).SemesterId);
        }
        protected virtual async Task<SelectList> GetSemesterListWithNoneAsync()
        {
            var semesters = await _db.Semesters.OrderByDescending(s => s.DateEnd).ToListAsync();
            var newList = new List<object> { new { SemesterId = -1, Name = "--Graduating Semester (optional)--" } };

            foreach (var s in semesters)
            {
                newList.Add(new
                {
                    s.SemesterId,
                    Name = s.ToString()
                });
            }

            return new SelectList(newList, "SemesterId", "Name");
        }
        protected virtual async Task<SelectList> GetUserIdListAsFullNameAsync()
        {
            var members = (await GetAllActivePledgeNeophyteMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName 
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetPledgeUserIdListAsFullNameAsync()
        {
            var members = (await GetAllPledgeMembersAsync()).OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.FirstName + " " + member.LastName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetUserIdListAsFullNameWithNoneAsync()
        {
            return new SelectList(await GetUserIdListAsFullNameWithNoneNonSelectListAsync(), "UserId", "Name");
        }
        protected virtual async Task<SelectList> GetStatusListAsync()
        {
            var statusList = await _db.MemberStatus.ToListAsync();
            var newList = new List<object>();
            foreach (var status in statusList)
            {
                newList.Add(new
                {
                    status.StatusId,
                    status.StatusName
                });
            }
            return new SelectList(newList, "StatusId", "StatusName");
        }
        protected virtual async Task<SelectList> GetStatusListWithNoneAsync()
        {
            var statusList = await _db.MemberStatus.ToListAsync();
            var newList = new List<object> { new { StatusId = -1, StatusName = "--Status (optional)--" } };
            foreach (var status in statusList)
            {
                newList.Add(new
                {
                    status.StatusId,
                    status.StatusName
                });
            }
            return new SelectList(newList, "StatusId", "StatusName");
        }
        protected virtual async Task<SelectList> GetPledgeClassListAsync()
        {
            var pledgeClasses = await _db.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
            var newList = new List<object>();

            foreach (var p in pledgeClasses)
            {
                newList.Add(new
                {
                    p.PledgeClassId,
                    p.PledgeClassName
                });
            }

            return new SelectList(newList, "PledgeClassId", "PledgeClassName");
        }
        protected virtual async Task<SelectList> GetPledgeClassListWithNoneAsync()
        {
            var pledgeClasses = await _db.PledgeClasses.OrderByDescending(s => s.Semester.DateEnd).ToListAsync();
            var newList = new List<object> { new { PledgeClassId = -1, PledgeClassName = "--Pledge Class (optional)--" } };

            foreach (var p in pledgeClasses)
            {
                newList.Add(new
                {
                    p.PledgeClassId,
                    p.PledgeClassName
                });
            }

            return new SelectList(newList, "PledgeClassId", "PledgeClassName");
        }
        protected virtual async Task<IEnumerable<Leader>> GetRecentAppointmentsAsync()
        {
            var thisAndComingSemesters = await _db.Semesters
                .Where(s => s.DateEnd >= DateTime.UtcNow)
                .OrderBy(s => s.DateStart)
                .ToListAsync();

            var leaders = _db.Leaders.ToList();

            var recentAppointsments = leaders
                .Where(l =>
                    l.SemesterId == thisAndComingSemesters[0].SemesterId ||
                    l.SemesterId == thisAndComingSemesters[1].SemesterId)
                .ToList();

            return recentAppointsments;
        }
        protected virtual async Task<double> GetRemainingServiceHoursForUserAsync(int userId)
        {
            const double requiredHours = 10;

            var lastSemester = await GetLastSemesterAsync();
            var currentSemester = await GetThisSemesterAsync();

            var totalHours = 0.0;
            try
            {
                var serviceHours = await _db.ServiceHours
                    .Where(h => h.Member.UserId == userId && 
                                h.Event.DateTimeOccurred > lastSemester.DateEnd && 
                                h.Event.DateTimeOccurred <= currentSemester.DateEnd)
                    .ToListAsync();
                if (serviceHours.Any())
                    totalHours = serviceHours.Select(h => h.DurationHours).Sum();
            }
            catch (Exception)
            {
                return requiredHours - totalHours < 0 ? 0 : requiredHours - totalHours;
            }

            var remainingHours = requiredHours - totalHours;
            return remainingHours < 0 ? 0 : remainingHours;
        }
        protected virtual SelectList GetAllEventIdsAsEventName()
        {
            // Beginning of today (12:00am of today).
            var today = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0));
            // Beginning of today, one year ago (12:00am of today).
            var yearAgoToday = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0));
            yearAgoToday -= new TimeSpan(365, 0, 0, 0);
            // Only retrieve events occuring within the last year.
            var events = _db.Events
                .Where(e => (e.DateTimeOccurred <= today && e.DateTimeOccurred >= yearAgoToday))
                .ToList();
            return new SelectList(events, "EventId", "EventName");
        }
        protected virtual async Task<SelectList> GetAllEventIdsAsEventNameAsync()
        {
            // Beginning of today (12:00am of today).
            var today = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0)); 
            // Beginning of today, one year ago (12:00am of today).
            var yearAgoToday = DateTimeFloor(DateTime.UtcNow, new TimeSpan(1, 0, 0, 0)); 
            yearAgoToday -= new TimeSpan(365, 0, 0, 0);
            // Only retrieve events occuring within the last year.
            var events = await _db.Events
                .Where(e => (e.DateTimeOccurred <= today && e.DateTimeOccurred >= yearAgoToday))
                .ToListAsync(); 
            return new SelectList(events, "EventId", "EventName");
        }
        protected virtual async Task<IEnumerable<ServiceHour>> GetAllCompletedEventsForUserAsync(int userId)
        {
            var thisSemester = await GetThisSemesterAsync();
            var lastSemester = await GetLastSemesterAsync();

            return await _db.ServiceHours
                .Where(e => e.UserId == userId && 
                            e.Event.DateTimeOccurred > lastSemester.DateEnd && 
                            e.Event.DateTimeOccurred <= thisSemester.DateEnd)
                .ToListAsync();
        }
        protected virtual async Task<IEnumerable<SoberSignup>> GetSoberSignupsForUserAsync(int userId, Semester semester)
        {
            var previousSemester = (await _db.Semesters
                .Where(s => s.DateEnd < semester.DateStart)
                .OrderBy(s => s.DateStart)
                .ToListAsync())
                .Last();

            return await _db.SoberSchedule
                .Where(s => s.UserId == userId && 
                            s.DateOfShift > previousSemester.DateEnd && 
                            s.DateOfShift <= semester.DateEnd)
                .ToListAsync();
        }
        protected virtual async Task<SelectList> GetAllApproverIdsAsync(int userId, Semester semester)
        {
            var members = await _db.Members.Where(a => a.UserId != userId).OrderBy(o => o.LastName).ToListAsync();
            // TODO: Refactor this to be more intelligent; requires discussion with academic chairman to decide who can approve.
            var membersOnStudyHours = (await _db.MemberStudyHourAssignments
                .Where(m =>
                    m.Assignment.Start >= semester.DateStart &&
                    m.Assignment.End <= semester.DateEnd)
                .ToListAsync())
                .Select(m => m.AssignedMember.UserId)
                .Distinct()
                .ToList();

            var newList = new List<object>();
            foreach (var member in members)
            {
                if (membersOnStudyHours.Contains(member.UserId)) continue;

                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual async Task<StudyHourAssignment> GetStudyHourAssignmentForUserAsync(int userId)
        {
            var member = await _db.Members.FindAsync(userId);

            return member.StudyHourAssignments
                .Select(a => a.Assignment)
                .SingleOrDefault(s => 
                    s.Start <= DateTime.UtcNow && 
                    s.End >= DateTime.UtcNow);
        }
        protected virtual async Task<IEnumerable<MemberStudyHourAssignment>> GetStudyHourAssignmentsForUserAsync(int userId, Semester semester)
        {
            var member = await _db.Members.FindAsync(userId);

            return member.StudyHourAssignments
                .Where(s =>
                    s.Assignment.Start >= semester.DateStart &&
                    s.Assignment.End <= semester.DateEnd)
                .OrderByDescending(s => s.Assignment.Start);
        }
        protected virtual async Task<SelectList> GetStudyHourAssignmentsSelectListForUserAsync(int userId, Semester semester)
        {
            var member = await _db.Members.FindAsync(userId);
            var memberAssignments = member.StudyHourAssignments
                .Where(s =>
                    s.Assignment.Start >= semester.DateStart &&
                    s.Assignment.End <= semester.DateEnd)
                .OrderByDescending(s => s.Assignment.Start);

            var newList = new List<object>();
            foreach (var a in memberAssignments)
            {
                newList.Add(new
                {
                    a.MemberStudyHourAssignmentId,
                    Name = ConvertUtcToCst(a.Assignment.Start) + " to " + ConvertUtcToCst(a.Assignment.End)
                });
            }

            return new SelectList(newList, "MemberStudyHourAssignmentId", "Name");
        }
        protected virtual DateTime GetStartOfCurrentWeek()
        {
            var now = ConvertUtcToCst(DateTime.UtcNow).Date;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return now.AddDays(-5);
                case DayOfWeek.Monday:
                    return now.AddDays(-6);
                case DayOfWeek.Tuesday:
                    return now;
                case DayOfWeek.Wednesday:
                    return now.AddDays(-1);
                case DayOfWeek.Thursday:
                    return now.AddDays(-2);
                case DayOfWeek.Friday:
                    return now.AddDays(-3);
                default: // Saturday
                    return now.AddDays(-4);
            }
        }
        protected virtual DateTime DateTimeFloor(DateTime date, TimeSpan span)
        {
            //Rounds down based on the TimeSpan
            //Ex. date = DateTime.UtcNow, span = TimeSpan of one day
            //Results in 12:00am of today
            var ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }
        protected virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
        protected virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }
        protected virtual SelectList GetShirtSizesSelectList()
        {
            var newList = new List<string> { "S", "M", "L", "XL", "2XL" };
            var list = new SelectList(newList.Select(x => new { Value = x, Text = x }), "Value", "Text");
            return list;
        }
        protected virtual SelectList GetTerms()
        {
            var terms = new List<string>
            {
                "Spring", "Fall"
            };
            return new SelectList(terms);
        }
        protected virtual SelectList GetPositionsList()
        {
            var positions = Roles.GetAllRoles();
            return new SelectList(positions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}