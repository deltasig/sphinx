namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MemberService : BaseService, IMemberService
{
    private readonly DspDbContext _context;
    private readonly ISemesterService _semesterService;

    public MemberService(DspDbContext context, ISemesterService semesterService)
    {
        _context = context;
        _semesterService = semesterService;
    }

    public async Task<Member> GetMemberByIdAsync(int id)
    {
        return await _context.FindAsync<Member>(id);
    }

    public async Task<Member> GetMemberByUserNameAsync(string userName)
    {
        return await _context.Users
            .Where(m => m.UserName == userName && m.MemberId != null)
            .Include(m => m.MemberInfo)
            .Select(x => x.MemberInfo)
            .SingleAsync();
    }

    public async Task<IEnumerable<Member>> GetAllMembersAsync()
    {
        var members = await _context.Members.ToListAsync();
        return members;
    }

    public async Task<IEnumerable<Member>> GetActivesAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId != semester.Id);
    }

    public async Task<IEnumerable<Member>> GetActivesAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetActivesAsync(semester);
    }

    public async Task<IEnumerable<Member>> GetActivesAsync(Semester semester)
    {
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId != semester.Id);
    }

    public async Task<IEnumerable<Member>> GetNewMembersAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId == semester.Id);
    }

    public async Task<IEnumerable<Member>> GetNewMembersAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetNewMembersAsync(semester);
    }

    public async Task<IEnumerable<Member>> GetNewMembersAsync(Semester semester)
    {
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId == semester.Id);
    }

    public async Task<IEnumerable<Member>> GetAlumniAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        return await GetAlumniAsync(semester);
    }

    public async Task<IEnumerable<Member>> GetAlumniAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetAlumniAsync(semester);
    }

    public async Task<IEnumerable<Member>> GetAlumniAsync(Semester asOfSemester)
    {
        var results = await _context.Members
            .Where(m => m.ExpectedGraduation.DateEnd < asOfSemester.DateStart)
            .OrderBy(m => m.LastName)
            .ToListAsync();

        return results;
    }

    public async Task<IEnumerable<Member>> GetCurrentRosterAsync()
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        return await GetRosterForSemesterAsync(currentSemester);
    }

    public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetRosterForSemesterAsync(semester);
    }

    public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester)
    {
        var results = await _context.Members
            .Where(d =>
                d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                d.ExpectedGraduation.DateEnd > semester.DateStart)
            .OrderBy(m => m.LastName)
            .Include(m => m.PledgeClass)
            .Include(m => m.UserInfo)
            .ToListAsync();
        return results;
    }

    public async Task<double> GetRemainingServiceHoursForUserAsync(int userId)
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        var priorSemester = await _semesterService.GetPriorSemesterAsync(currentSemester);

        var requiredHours = currentSemester.MinimumServiceHours;
        var totalHours = 0.0;
        try
        {
            var serviceHours = await _context.ServiceHours
                .Where(h => h.User.Id == userId &&
                            h.Event.DateTimeOccurred > priorSemester.DateEnd &&
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

    public async Task<IEnumerable<ServiceHour>> GetAllCompletedEventsForUserAsync(int userId)
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        var lastSemester = await _semesterService.GetPriorSemesterAsync(currentSemester);

        return await _context.ServiceHours
            .Where(e => e.UserId == userId &&
                        e.Event.DateTimeOccurred > lastSemester.DateEnd &&
                        e.Event.DateTimeOccurred <= currentSemester.DateEnd)
            .ToListAsync();
    }

    public async Task<IEnumerable<SoberSignup>> GetSoberSignupsForUserAsync(int userId, Semester semester)
    {
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        var priorSemester = await _semesterService.GetPriorSemesterAsync(currentSemester);

        return await _context.SoberSignups
            .Where(s => s.UserId == userId &&
                        s.DateOfShift > priorSemester.DateEnd &&
                        s.DateOfShift <= semester.DateEnd)
            .ToListAsync();
    }
}
