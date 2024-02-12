namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMemberService : IService
{
    Task<Member> GetMemberByIdAsync(int id);
    Task<Member> GetMemberByUserNameAsync(string userName);
    Task<IEnumerable<Member>> GetAllMembersAsync();
    Task<IEnumerable<Member>> GetActivesAsync();
    Task<IEnumerable<Member>> GetActivesAsync(int semesterId);
    Task<IEnumerable<Member>> GetActivesAsync(Semester semester);
    Task<IEnumerable<Member>> GetNewMembersAsync();
    Task<IEnumerable<Member>> GetNewMembersAsync(int semesterId);
    Task<IEnumerable<Member>> GetNewMembersAsync(Semester semester);
    Task<IEnumerable<Member>> GetAlumniAsync();
    Task<IEnumerable<Member>> GetAlumniAsync(int semesterId);
    Task<IEnumerable<Member>> GetAlumniAsync(Semester semester);
    Task<IEnumerable<Member>> GetCurrentRosterAsync();
    Task<IEnumerable<Member>> GetRosterForSemesterAsync(int semesterId);
    Task<IEnumerable<Member>> GetRosterForSemesterAsync(Semester semester);
    Task<double> GetRemainingServiceHoursForUserAsync(int userId);
    Task<IEnumerable<ServiceHour>> GetAllCompletedEventsForUserAsync(int userId);
    Task<IEnumerable<SoberSignup>> GetSoberSignupsForUserAsync(int userId, Semester semester);
}