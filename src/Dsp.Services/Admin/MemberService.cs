namespace Dsp.Services.Admin
{
    using Data;
    using Data.Entities;
    using Interfaces;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class MemberService : BaseService, IMemberService
    {
        private readonly ISemesterService _semesterService;

        public MemberService(SphinxDbContext db) : base(db)
        {
            _semesterService = new SemesterService(db);
        }

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _db.Users.SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Member> GetMemberByUserNameAsync(string userName)
        {
            return await _db.Users.SingleOrDefaultAsync(m => m.UserName == userName);
        }

        public async Task<IEnumerable<Member>> GetActivesAsync()
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            var roster = await GetRosterForSemesterAsync(semester);
            return roster.Where(m => m.PledgeClass.SemesterId != semester.SemesterId);
        }

        public async Task<IEnumerable<Member>> GetActivesAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetActivesAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetActivesAsync(Semester semester)
        {
            var roster = await GetRosterForSemesterAsync(semester);
            return roster.Where(m => m.PledgeClass.SemesterId != semester.SemesterId);
        }

        public async Task<IEnumerable<Member>> GetNewMembersAsync()
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            var roster = await GetRosterForSemesterAsync(semester);
            return roster.Where(m => m.PledgeClass.SemesterId == semester.SemesterId);
        }

        public async Task<IEnumerable<Member>> GetNewMembersAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetNewMembersAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetNewMembersAsync(Semester semester)
        {
            var roster = await GetRosterForSemesterAsync(semester);
            return roster.Where(m => m.PledgeClass.SemesterId == semester.SemesterId);
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
            return await _db.Users
                .Where(m => (
                    m.MemberStatus.StatusName == "Advisor" ||
                    (m.MemberStatus.StatusName == "Released" ||
                    m.MemberStatus.StatusName == "Alumnus" ||
                    m.MemberStatus.StatusName == "Neophyte" ||
                    m.MemberStatus.StatusName == "Active" ||
                    m.MemberStatus.StatusName == "Pledge") &&
                    m.GraduationSemester.DateEnd < semester.DateStart))
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetRosterForSemesterAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester)
        {
            return await _db.Users
                .Where(d =>
                    d.MemberStatus.StatusName == "Advisor" ||
                    ((d.MemberStatus.StatusName == "Released" ||
                    d.MemberStatus.StatusName == "Alumnus" ||
                    d.MemberStatus.StatusName == "Neophyte" ||
                    d.MemberStatus.StatusName == "Active" ||
                    d.MemberStatus.StatusName == "Pledge") &&
                    d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                    d.GraduationSemester.DateEnd > semester.DateStart))
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }
    }
}
