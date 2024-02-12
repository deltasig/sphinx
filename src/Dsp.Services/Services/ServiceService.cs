namespace Dsp.Services;

using Data;
using Dsp.Data.Entities;
using Dsp.Services.Models;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

public class ServiceService : BaseService, IServiceService
{
    private readonly DspDbContext _context;
    private readonly IMemberService _memberService;
    private readonly ISemesterService _semesterService;

    public ServiceService(DspDbContext context, IMemberService memberService,
        ISemesterService semesterService)
    {
        _context = context;
        _memberService = memberService;
        _semesterService = semesterService;
    }

    public async Task<IEnumerable<ServiceEvent>> GetEventsBySemesterIdAsync(int sid)
    {
        var events = await _context.ServiceEvents
            .Where(x => x.SemesterId == sid)
            .OrderByDescending(x => x.DateTimeOccurred)
            .ToListAsync();

        return events;
    }

    public async Task<IEnumerable<ServiceEvent>> GetEventsForSemesterAsync(Semester semester)
    {
        var events = await GetEventsBySemesterIdAsync(semester.Id);

        return events;
    }

    public async Task<IEnumerable<Semester>> GetSemestersWithEventsAsync(Semester currentSemester, bool includeCurrent = true)
    {
        var semesters = await _context.Semesters
            .Where(x => x.ServiceEvents.Any())
            .OrderByDescending(x => x.DateStart)
            .ToListAsync();

        if (includeCurrent && !semesters.Any(x => x.Id == currentSemester.Id))
        {
            semesters = semesters
                .Concat(new List<Semester> { currentSemester })
                .OrderByDescending(x => x.DateStart)
                .ToList();
        }

        return semesters;
    }

    public async Task<ServiceEvent> GetEventByIdAsync(int id)
    {
        return await _context.FindAsync<ServiceEvent>(id);
    }

    public async Task<ServiceHour> GetHoursAsync(int eventId, int userId)
    {
        return await _context.ServiceHours
            .Where(x => x.EventId == eventId && x.UserId == userId)
            .SingleAsync();
    }

    public async Task<IEnumerable<ServiceEventAmendment>> GetEventAmendmentsBySemesterIdAsync(int semesterId)
    {
        return await _context.ServiceEventAmendments
            .Where(x => x.SemesterId == semesterId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceHourAmendment>> GetHoursAmendmentsBySemesterIdAsync(int semesterId)
    {
        return await _context.ServiceHourAmendments
            .Where(x => x.SemesterId == semesterId)
            .ToListAsync();
    }

    public async Task<ServiceEventAmendment> GetEventAmendmentByIdAsync(int id)
    {
        return await _context.FindAsync<ServiceEventAmendment>(id);
    }

    public async Task<ServiceHourAmendment> GetHoursAmendmentByIdAsync(int id)
    {
        return await _context.FindAsync<ServiceHourAmendment>(id);
    }


    public async Task CreateEventAsync(ServiceEvent serviceEvent)
    {
        _context.Add(serviceEvent);
        await _context.SaveChangesAsync();
    }

    public async Task CreateHoursAsync(ServiceHour serviceHours)
    {
        _context.Add(serviceHours);
        await _context.SaveChangesAsync();
    }

    public async Task CreateEventAmendmentAsync(ServiceEventAmendment eventAmendment)
    {
        _context.Add(eventAmendment);
        await _context.SaveChangesAsync();
    }

    public async Task CreateHoursAmendmentAsync(ServiceHourAmendment hoursAmendment)
    {
        _context.Add(hoursAmendment);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateEventAsync(ServiceEvent serviceEvent)
    {
        _context.Update(serviceEvent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateHoursAsync(ServiceHour serviceHours)
    {
        _context.Update(serviceHours);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteEventByIdAsync(int id)
    {
        var entity = new ServiceEvent { EventId = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHoursAsync(ServiceHour serviceHours)
    {
        _context.Remove(serviceHours);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEventAmendmentByIdAsync(int id)
    {
        var entity = new ServiceEventAmendment { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHoursAmendmentByIdAsync(int id)
    {
        var entity = new ServiceHourAmendment { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }


    public async Task<IList<ServiceGeneralHistoricalStats>> GetGeneralHistoricalStatsAsync()
    {
        // Check the cache
        ObjectCache cache = MemoryCache.Default;
        var cacheKey = "general_historical_stats";
        if (cache[cacheKey] is IList<ServiceGeneralHistoricalStats> generalHistoricalStats)
            return generalHistoricalStats;

        // Calculate stats
        generalHistoricalStats = new List<ServiceGeneralHistoricalStats>();
        var semesters = await _semesterService.GetAllSemestersAsync();
        var semestersWithApprovedEvents = semesters
            .Where(x => x.ServiceEvents.Any(e => e.IsApproved))
            .OrderByDescending(x => x.DateStart);
        foreach (var sem in semestersWithApprovedEvents)
        {
            var roster = await _memberService.GetRosterForSemesterAsync(sem.Id);
            var nonExemptRoster = roster
                .Where(x => x.ServiceHourAmendments
                    .Where(s => s.SemesterId == sem.Id)
                    .Sum(h => h.AmountHours) + sem.MinimumServiceHours > 0);
            var calculatedOn = ConvertUtcToCst(DateTime.UtcNow);
            var semesterStats = new ServiceGeneralHistoricalStats(sem, nonExemptRoster, calculatedOn);
            generalHistoricalStats.Add(semesterStats);
        }

        // Add to cache
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(6) };
        cache.Set(cacheKey, generalHistoricalStats, cacheItemPolicy);

        return generalHistoricalStats;
    }

    public async Task<IEnumerable<ServiceMemberProgress>> GetRosterProgressBySemesterIdAsync(int sid)
    {
        // Check the cache
        ObjectCache cache = MemoryCache.Default;
        var cacheKey = $"roster_progress_stats_{sid}";
        if (cache[cacheKey] is IList<ServiceMemberProgress> rosterProgress)
            return rosterProgress;

        // Calculate progress
        var selectedSemester = await _semesterService.GetSemesterByIdAsync(sid);
        var roster = await _memberService.GetRosterForSemesterAsync(sid);
        rosterProgress = new List<ServiceMemberProgress>();
        foreach (var m in roster)
        {
            var calculatedOn = ConvertUtcToCst(DateTime.UtcNow);
            var memberProgress = new ServiceMemberProgress(selectedSemester, m, calculatedOn);
            rosterProgress.Add(memberProgress);
        }

        // Add to cache
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(7) };
        if (selectedSemester.Id == currentSemester.Id)
        {
            cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(6) };
        }
        cache.Set(cacheKey, rosterProgress, cacheItemPolicy);

        return rosterProgress;
    }

    public async Task<ServiceMemberStats> GetMemberStatsBySemesterIdAsync(int sid)
    {
        // Check the cache
        ObjectCache cache = MemoryCache.Default;
        var cacheKey = $"member_stats_{sid.ToString()}";
        if (cache[cacheKey] is ServiceMemberStats memberStats)
            return memberStats;

        // Calculate stats
        var selectedSemester = await _semesterService.GetSemesterByIdAsync(sid);
        var rosterProgress = await GetRosterProgressBySemesterIdAsync(sid);

        var exemptMembers = rosterProgress
            .Where(m => m.HourAmendmentsTotal + selectedSemester.MinimumServiceHours <= 0)
            .OrderBy(m => m.LastName)
            .ToList();
        var exemptMemberCount = exemptMembers.Count;
        var nonExemptMembers = rosterProgress
            .Where(m => m.HourAmendmentsTotal + selectedSemester.MinimumServiceHours > 0).ToList();
        var nonExemptMemberCount = nonExemptMembers.Count;
        var actives = rosterProgress
            .Where(m => m.NewMemberClassSemesterId != selectedSemester.Id)
            .ToList();
        var nonExemptActiveMembers = actives
            .Where(m => !exemptMembers.Select(e => e.MemberId).Contains(m.MemberId))
            .ToList();
        var nonExemptActiveMemberCount = nonExemptActiveMembers.Count;
        var newMembers = rosterProgress
            .Where(m => m.NewMemberClassSemesterId == selectedSemester.Id)
            .ToList();
        var nonExemptNewMembers = newMembers
            .Where(m => !exemptMembers.Select(e => e.MemberId).Contains(m.MemberId))
            .ToList();
        var nonExemptNewMemberCount = nonExemptNewMembers.Count;

        var allMembersServed = nonExemptMembers.Count(m => m.Hours > 0);
        var allMembersServedPercentage = (int)(100 * (allMembersServed / (double)nonExemptMemberCount));
        var allMembersServedTen = nonExemptMembers.Count(m => m.Hours >= 10);
        var allMembersServedTenPercentage = (int)(100 * (allMembersServedTen / (double)nonExemptMemberCount));
        var allMembersServedFifteen = nonExemptMembers.Count(m => m.Hours >= 15);
        var allMembersServedFifteenPercentage = (int)(100 * (allMembersServedFifteen / (double)nonExemptMemberCount));
        var totalMemberHours = nonExemptMembers.Sum(m => m.Hours);
        var averageAllMemberHours = (totalMemberHours / nonExemptMemberCount).ToString("F1");
        var averageAllMemberAttendance = nonExemptMembers.Count(m => m.ServiceHoursCount >= m.EventAmendmentsTotal + selectedSemester.MinimumServiceEvents);
        var averageAllMemberAttendancePercentage = (int)(100 * (averageAllMemberAttendance / (double)(nonExemptMemberCount == 0 ? 1 : nonExemptMemberCount)));

        var activeMembersServed = nonExemptActiveMembers.Count(m => m.Hours > 0);
        var activeMembersServedPercentage = (int)(100 * (activeMembersServed / (double)(nonExemptActiveMemberCount == 0 ? 1 : nonExemptActiveMemberCount)));
        var activeMembersServedTen = nonExemptActiveMembers.Count(m => m.Hours >= 10);
        var activeMembersServedTenPercentage = (int)(100 * (activeMembersServedTen / (double)(nonExemptActiveMemberCount == 0 ? 1 : nonExemptActiveMemberCount)));
        var activeMembersServedFifteen = nonExemptActiveMembers.Count(m => m.Hours >= 15);
        var activeMembersServedFifteenPercentage = (int)(100 * (activeMembersServedFifteen / (double)(nonExemptActiveMemberCount == 0 ? 1 : nonExemptActiveMemberCount)));
        var totalActiveMemberHours = actives.Sum(m => m.Hours);
        var averageActiveMemberHours = (totalActiveMemberHours / (nonExemptActiveMemberCount == 0 ? 1 : nonExemptActiveMemberCount)).ToString("F1");
        var averageActiveMemberAttendance = nonExemptActiveMembers.Count(m => m.ServiceHoursCount >= m.EventAmendmentsTotal + selectedSemester.MinimumServiceEvents);
        var averageActiveMemberAttendancePercentage = (int)(100 * (averageActiveMemberAttendance / (double)(nonExemptActiveMemberCount == 0 ? 1 : nonExemptActiveMemberCount)));

        var newMembersServed = nonExemptNewMembers.Count(m => m.Hours > 0);
        var newMembersServedPercentage = (int)(100 * (newMembersServed / (double)(nonExemptNewMembers.Count == 0 ? 1 : nonExemptNewMembers.Count)));
        var newMembersServedTen = nonExemptNewMembers.Count(m => m.Hours >= 10);
        var newMembersServedTenPercentage = (int)(100 * (newMembersServedTen / (double)(nonExemptNewMembers.Count == 0 ? 1 : nonExemptNewMembers.Count)));
        var newMembersServedFifteen = nonExemptNewMembers.Count(m => m.Hours >= 15);
        var newMembersServedFifteenPercentage = (int)(100 * (newMembersServedFifteen / (double)(nonExemptNewMembers.Count == 0 ? 1 : nonExemptNewMembers.Count)));
        var totalNewMemberHours = newMembers.Sum(m => m.Hours);
        var averageNewMemberHours = (totalNewMemberHours / (nonExemptNewMembers.Count == 0 ? 1 : nonExemptNewMembers.Count)).ToString("F1");
        var averageNewMemberAttendance = nonExemptNewMembers.Count(m => m.ServiceHoursCount >= m.EventAmendmentsTotal + selectedSemester.MinimumServiceEvents);
        var averageNewMemberAttendancePercentage = (int)(100 * (averageNewMemberAttendance / (double)(nonExemptNewMembers.Count == 0 ? 1 : nonExemptNewMembers.Count)));
        var exemptMembersDisplay = string.Empty;
        for (var i = 0; i < exemptMembers.Count; i++)
        {
            exemptMembersDisplay += $"{exemptMembers[i].FirstName} {exemptMembers[i].LastName}";
            if (i < exemptMembers.Count - 1)
            {
                exemptMembersDisplay += ", ";
            }
        }

        var calculatedOn = ConvertUtcToCst(DateTime.UtcNow);
        memberStats = new ServiceMemberStats(
            nonExemptMemberCount,
            exemptMemberCount,
            nonExemptActiveMemberCount,
            nonExemptNewMemberCount,
            totalMemberHours,
            totalActiveMemberHours,
            totalNewMemberHours,
            allMembersServed,
            allMembersServedPercentage,
            activeMembersServed,
            activeMembersServedPercentage,
            newMembersServed,
            newMembersServedPercentage,
            allMembersServedTen,
            allMembersServedTenPercentage,
            activeMembersServedTen,
            activeMembersServedTenPercentage,
            newMembersServedTen,
            newMembersServedTenPercentage,
            allMembersServedFifteen,
            allMembersServedFifteenPercentage,
            activeMembersServedFifteen,
            activeMembersServedFifteenPercentage,
            newMembersServedFifteen,
            newMembersServedFifteenPercentage,
            averageAllMemberHours,
            averageActiveMemberHours,
            averageNewMemberHours,
            averageAllMemberAttendance,
            averageAllMemberAttendancePercentage,
            averageActiveMemberAttendance,
            averageActiveMemberAttendancePercentage,
            averageNewMemberAttendance,
            averageNewMemberAttendancePercentage,
            exemptMembersDisplay,
            calculatedOn
        );

        // Add model to cache
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(7) };
        if (selectedSemester.Id == currentSemester.Id)
        {
            cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(6) };
        }
        cache.Set(cacheKey, memberStats, cacheItemPolicy);

        return memberStats;
    }

    public async Task<ServiceHourStats> GetHourStatsBySemesterIdAsync(int sid)
    {
        // Check the cache
        ObjectCache cache = MemoryCache.Default;
        var cacheKey = $"hour_stats_{sid.ToString()}";
        if (cache[cacheKey] is ServiceHourStats hourStats)
            return hourStats;

        // Calculate stats
        var selectedSemester = await _semesterService.GetSemesterByIdAsync(sid);
        var priorSemester = await _semesterService.GetPriorSemesterAsync(selectedSemester);
        var roster = await _memberService.GetRosterForSemesterAsync(sid);
        var unadjustedMemberCount = roster.Count();
        roster = roster.Where(x => x.ServiceHourAmendments
                .Where(s => s.SemesterId == selectedSemester.Id)
                .Sum(h => h.AmountHours) + selectedSemester.MinimumServiceHours > 0);
        var adjustedMemberCount = roster.Count();
        var events = await GetEventsBySemesterIdAsync(sid);
        var eventCount = events.Count();
        var now = DateTime.UtcNow;
        var monthYearTuples = MonthsBetween(priorSemester.DateEnd, selectedSemester.DateEnd);
        var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

        var months = monthYearTuples.Select(x => $"{dateTimeFormat.GetMonthName(x.Item1)} {x.Item2}");
        var moreThanZeroHoursSeries = new List<int>();
        var fiveOrMoreHoursSeries = new List<int>();
        var tenOrMoreHoursSeries = new List<int>();
        var fifteenOrMoreHoursSeries = new List<int>();
        var membersToHours = new Dictionary<int, double>();

        foreach (var monthYear in monthYearTuples)
        {
            var firstDayOfMonth = new DateTime(monthYear.Item2, monthYear.Item1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (now < firstDayOfMonth) continue;

            var monthEvents = events
                .Where(x => x.IsApproved &&
                    x.DateTimeOccurred.Month == monthYear.Item1 &&
                    x.DateTimeOccurred.Year == monthYear.Item2);
            // Add the month's submissions to lookup
            foreach (var e in monthEvents)
            {
                foreach (var h in e.ServiceHours)
                {
                    if (!roster.Any(x => x.Id == h.UserId)) continue;

                    if (!membersToHours.ContainsKey(h.UserId))
                        membersToHours.Add(h.UserId, 0.0);

                    membersToHours[h.UserId] += h.DurationHours;
                }
            }

            // Check lookup results to populate lists
            moreThanZeroHoursSeries.Add(membersToHours.Count);
            fiveOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 5));
            tenOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 10));
            fifteenOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 15));
        }

        var calculatedOn = ConvertUtcToCst(DateTime.UtcNow);
        hourStats = new ServiceHourStats(
            unadjustedMemberCount,
            adjustedMemberCount,
            months,
            moreThanZeroHoursSeries,
            fiveOrMoreHoursSeries,
            tenOrMoreHoursSeries,
            fifteenOrMoreHoursSeries,
            calculatedOn
        );

        // Add model to cache
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromDays(7) };
        if (selectedSemester.Id == currentSemester.Id)
        {
            cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(6) };
        }
        cache.Set(cacheKey, hourStats, cacheItemPolicy);

        return hourStats;
    }

    private static IEnumerable<Tuple<int, int>> MonthsBetween(DateTime startDate, DateTime endDate)
    {
        DateTime iterator;
        DateTime limit;

        if (endDate > startDate)
        {
            iterator = new DateTime(startDate.Year, startDate.Month, 1);
            limit = endDate;
        }
        else
        {
            iterator = new DateTime(endDate.Year, endDate.Month, 1);
            limit = startDate;
        }
        while (iterator <= limit)
        {
            yield return Tuple.Create(iterator.Month, iterator.Year);
            iterator = iterator.AddMonths(1);
        }
    }
}
