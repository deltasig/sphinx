namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRoleService : IService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role> GetRoleByIdAsync(int id);
    Task<Role> GetRoleByNameAsync(string roleName);
    Task<User> GetUserInRoleAsync(string roleName, int semesterId);
    Task<IEnumerable<Role>> GetAppointmentsAsync(int semesterId);
    Task<IEnumerable<Role>> GetEboardRolesAsync();
    Task<IEnumerable<Role>> GetCurrentRolesByUserIdAsync(int userId);
    Task<IEnumerable<Role>> GetCurrentRolesByUserNameAsync(string userName);
    Task<Role> GetAdminRoleAsync();
    Task<UserRole> GetAdminUserRoleForUserAsync(int userId);

    Task<bool> UserIsAdminAsync(int userId);
    Task<bool> UserIsAdminAsync(string userName);
    Task<bool> UserIsCurrentlyInRoleAsync(int userId, int roleId);
    Task<bool> UserIsCurrentlyInRoleAsync(int userId, string roleName);
    Task<bool> UserIsCurrentlyInRoleAsync(string userName, string roleName);
    Task<bool> UserIsCurrentlyInAtLeastOneRoleAsync(int userId, string[] roleNames);

    Task CreateRoleAsync(Role role);
    Task AssignUserToSemesterRoleAsync(int userId, int roleId, int semesterId);

    Task UpdateRoleAsync(Role role);

    Task RemoveRoleAsync(Role role);
    Task RemoveRoleByIdAsync(int id);
    Task RemoveUserFromSemesterRoleAsync(int userId, int roleId, int semesterId);
}
