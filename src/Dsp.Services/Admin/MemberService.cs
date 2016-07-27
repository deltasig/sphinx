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

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetRosterForSemesterAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester)
        {
            return await _db.Users
                .Where(d =>
                    d.LastName != "Hirtz" &&
                    (d.MemberStatus.StatusName == "Released" ||
                    d.MemberStatus.StatusName == "Alumnus" ||
                    d.MemberStatus.StatusName == "Active" ||
                    d.MemberStatus.StatusName == "Pledge") &&
                    d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                    d.GraduationSemester.DateEnd > semester.DateStart)
                .ToListAsync();
        }
    }
}
