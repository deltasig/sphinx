namespace Dsp.Services
{
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

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _context.FindAsync<Member>(id);
        }

        public async Task<Member> GetMemberByUserNameAsync(string userName)
        {
            return await _context.Users
                .Where(m => m.UserName == userName)
                .SingleAsync();
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

        public async Task<IEnumerable<Member>> GetAlumniAsync(Semester semester)
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

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetRosterForSemesterAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester)
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
                .ToListAsync();
            return results;
        }
    }
}
