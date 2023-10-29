namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MemberService : BaseService, IMemberService
{
    private readonly DspDbContext _context;
    private readonly ISemesterService _semesterService;

    public MemberService(DspDbContext context)
    {
        _context = context;
        _semesterService = new SemesterService(context);
    }

    public async Task<User> GetMemberByIdAsync(int id)
    {
        return await _context.FindAsync<User>(id);
    }

    public async Task<User> GetMemberByUserNameAsync(string userName)
    {
        return await _context.Users
            .Where(m => m.UserName == userName)
            .Include(m => m.Status)
            .SingleAsync();
    }

    public async Task<IEnumerable<User>> GetActivesAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId != semester.Id);
    }

    public async Task<IEnumerable<User>> GetActivesAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetActivesAsync(semester);
    }

    public async Task<IEnumerable<User>> GetActivesAsync(Semester semester)
    {
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId != semester.Id);
    }

    public async Task<IEnumerable<User>> GetNewMembersAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId == semester.Id);
    }

    public async Task<IEnumerable<User>> GetNewMembersAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetNewMembersAsync(semester);
    }

    public async Task<IEnumerable<User>> GetNewMembersAsync(Semester semester)
    {
        var roster = await GetRosterForSemesterAsync(semester);
        return roster.Where(m => m.PledgeClass.SemesterId == semester.Id);
    }

    public async Task<IEnumerable<User>> GetAlumniAsync()
    {
        var semester = await _semesterService.GetCurrentSemesterAsync();
        return await GetAlumniAsync(semester);
    }

    public async Task<IEnumerable<User>> GetAlumniAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetAlumniAsync(semester);
    }

    public async Task<IEnumerable<User>> GetAlumniAsync(Semester semester)
    {
        var results = await _context.Users
            .Where(m =>
                m.Status.StatusName == "Advisor" ||
                (m.Status.StatusName == "Released" ||
                m.Status.StatusName == "Alumnus" ||
                m.Status.StatusName == "Neophyte" ||
                m.Status.StatusName == "Active" ||
                m.Status.StatusName == "New") &&
                m.ExpectedGraduation.DateEnd < semester.DateStart)
            .OrderBy(m => m.LastName)
            .ToListAsync();

        return results;
    }

    public async Task<IEnumerable<User>> GetRosterForSemesterAsync(int semesterId)
    {
        var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
        return await GetRosterForSemesterAsync(semester);
    }

    public async Task<IEnumerable<User>> GetRosterForSemesterAsync(Semester semester)
    {
        var results = await _context.Users
            .Where(d =>
                d.Status.StatusName != "Advisor" &&
                (d.Status.StatusName == "Alumnus" ||
                d.Status.StatusName == "Active" ||
                d.Status.StatusName == "Neophyte" ||
                d.Status.StatusName == "New") &&
                d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                d.ExpectedGraduation.DateEnd > semester.DateStart)
            .OrderBy(m => m.LastName)
            .Include(m => m.PledgeClass)
            .ToListAsync();
        return results;
    }
}
