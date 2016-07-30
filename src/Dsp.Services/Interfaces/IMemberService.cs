namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMemberService
    {
        Task<Member> GetMemberByIdAsync(int id);
        Task<Member> GetMemberByUserNameAsync(string userName);
        Task<IEnumerable<Member>> GetActivesAsync();
        Task<IEnumerable<Member>> GetActivesAsync(int semesterId);
        Task<IEnumerable<Member>> GetActivesAsync(Semester semester);
        Task<IEnumerable<Member>> GetNewMembersAsync();
        Task<IEnumerable<Member>> GetNewMembersAsync(int semesterId);
        Task<IEnumerable<Member>> GetNewMembersAsync(Semester semester);
        Task<IEnumerable<Member>> GetAlumniAsync();
        Task<IEnumerable<Member>> GetAlumniAsync(int semesterId);
        Task<IEnumerable<Member>> GetAlumniAsync(Semester semester);
        Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId);
        Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester);
    }
}