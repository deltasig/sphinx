namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPositionService
    {
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task<Position> GetPositionByIdAsync(int id);
        Task<Position> GetPositionByNameAsync(string name);
        Task RemovePositionAsync(Position entity);
        Task RemovePositionByIdAsync(int id);
        Task CreatePositionAsync(Position entity);
        Task UpdatePositionAsync(Position entity);
        Task<IEnumerable<Position>> GetAppointmentsAsync(int sid);
        Task AppointMemberToPositionAsync(int mid, int pid, int sid);
        Task RemoveMemberFromPositionAsync(int mid, int pid, int sid);
    }
}