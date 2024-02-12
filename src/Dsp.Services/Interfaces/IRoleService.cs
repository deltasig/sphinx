namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRoleService : IService
{
    Task<IEnumerable<Position>> GetAllRolesAsync();
    Task<Position> GetRoleByIdAsync(int id);
    Task<Position> GetRoleByNameAsync(string roleName);
    Task<Member> GetUserInRoleAsync(string roleName, int semesterId);
    Task<IEnumerable<Position>> GetAppointmentsAsync(int semesterId);
    Task<IEnumerable<Position>> GetEboardRolesAsync();
    Task<IEnumerable<Position>> GetCurrentRolesByUserIdAsync(int userId);
    Task<IEnumerable<Position>> GetCurrentRolesByUserNameAsync(string userName);
    Task<Position> GetAdminRoleAsync();
    Task<MemberPosition> GetAdminUserRoleForUserAsync(int userId);

    Task<bool> UserIsAdminAsync(int userId);
    Task<bool> UserIsAdminAsync(string userName);
    Task<bool> UserIsCurrentlyInRoleAsync(int userId, int roleId);
    Task<bool> UserIsCurrentlyInRoleAsync(int userId, string roleName);
    Task<bool> UserIsCurrentlyInRoleAsync(string userName, string roleName);
    Task<bool> UserIsCurrentlyInAtLeastOneRoleAsync(int userId, string[] roleNames);

    Task CreateRoleAsync(Position role);
    Task AssignUserToSemesterRoleAsync(int userId, int roleId, int semesterId);

    Task UpdateRoleAsync(Position role);

    Task RemoveRoleAsync(Position role);
    Task RemoveRoleByIdAsync(int id);
    Task RemoveUserFromSemesterRoleAsync(int userId, int roleId, int semesterId);
}
