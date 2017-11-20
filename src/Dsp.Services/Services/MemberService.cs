namespace Dsp.Services
{
    using Data;
    using Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MemberService : BaseService, IMemberService
    {
        private readonly IRepository _repository;
        private readonly ISemesterService _semesterService;

        public MemberService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public MemberService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public MemberService(IRepository repository)
        {
            _repository = repository;
            _semesterService = new SemesterService(repository);
        }

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<Member>(id);
        }

        public async Task<Member> GetMemberByUserNameAsync(string userName)
        {
            return await _repository.GetOneAsync<Member>(m => m.UserName == userName);
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
            var results = await _repository
                .GetAsync<Member>(m =>
                    m.MemberStatus.StatusName == "Advisor" ||
                    (m.MemberStatus.StatusName == "Released" ||
                    m.MemberStatus.StatusName == "Alumnus" ||
                    m.MemberStatus.StatusName == "Neophyte" ||
                    m.MemberStatus.StatusName == "Active" ||
                    m.MemberStatus.StatusName == "Pledge") &&
                    m.GraduationSemester.DateEnd < semester.DateStart);
            return results.OrderBy(m => m.LastName);

        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(semesterId);
            return await GetRosterForSemesterAsync(semester);
        }

        public async Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester)
        {
            var results = await _repository
                .GetAsync<Member>(d =>
                    d.MemberStatus.StatusName == "Advisor" ||
                    ((d.MemberStatus.StatusName == "Released" ||
                    d.MemberStatus.StatusName == "Alumnus" ||
                    d.MemberStatus.StatusName == "Neophyte" ||
                    d.MemberStatus.StatusName == "Active" ||
                    d.MemberStatus.StatusName == "Pledge") &&
                    d.PledgeClass.Semester.DateStart < semester.DateEnd &&
                    d.GraduationSemester.DateEnd > semester.DateStart));
            return results.OrderBy(m => m.LastName);
        }
    }
}
