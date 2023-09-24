namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPositionService : IService
    {
        Task<IEnumerable<Role>> GetAllPositionsAsync();
        Task<Role> GetPositionByIdAsync(int id);
        Task<Role> GetPositionByNameAsync(string name);
        Task<User> GetUserInPositionAsync(string positionName, int sid);
        Task<IEnumerable<Role>> GetAppointmentsAsync(int sid);
        Task<IEnumerable<Role>> GetEboardPositionsAsync();
        Task<IEnumerable<Role>> GetCurrentPositionsByUserAsync(int userId);
        Task<IEnumerable<Role>> GetCurrentPositionsByUserAsync(string userName);
        Task<Role> GetAdminPositionAsync();
        Task<UserRole> GetAdminAppoinmentForUserAsync(int userId);

        Task<bool> UserIsAdminAsync(int userId);
        Task<bool> UserIsAdminAsync(string userName);
        Task<bool> UserHasPositionPowerAsync(int userId, int positionId);
        Task<bool> UserHasPositionPowerAsync(int userId, string positionName);
        Task<bool> UserHasPositionPowerAsync(string userName, string positionName);
        Task<bool> UserHasAtLeastOnePositionPowerAsync(int userId, string[] positionNames);

        Task CreatePositionAsync(Role entity);
        Task AppointMemberToPositionAsync(int mid, int pid, int sid);

        Task UpdatePositionAsync(Role entity);

        Task RemovePositionAsync(Role entity);
        Task RemovePositionByIdAsync(int id);
        Task RemoveMemberFromPositionAsync(int mid, int pid, int sid);
    }
}