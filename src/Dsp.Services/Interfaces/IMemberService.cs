namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMemberService : IService
{
    Task<User> GetMemberByIdAsync(int id);
    Task<User> GetMemberByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetActivesAsync();
    Task<IEnumerable<User>> GetActivesAsync(int semesterId);
    Task<IEnumerable<User>> GetActivesAsync(Semester semester);
    Task<IEnumerable<User>> GetNewMembersAsync();
    Task<IEnumerable<User>> GetNewMembersAsync(int semesterId);
    Task<IEnumerable<User>> GetNewMembersAsync(Semester semester);
    Task<IEnumerable<User>> GetAlumniAsync();
    Task<IEnumerable<User>> GetAlumniAsync(int semesterId);
    Task<IEnumerable<User>> GetAlumniAsync(Semester semester);
    Task<IEnumerable<User>> GetRosterForSemesterAsync(int semesterId);
    Task<IEnumerable<User>> GetRosterForSemesterAsync(Semester semester);
}