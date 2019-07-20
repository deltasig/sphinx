namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPositionService : IService
    {
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task<Position> GetPositionByIdAsync(int id);
        Task<Position> GetPositionByNameAsync(string name);
        Task<Member> GetUserInPositionAsync(string positionName, int sid);
        Task<IEnumerable<Position>> GetAppointmentsAsync(int sid);
        Task<IEnumerable<Position>> GetEboardPositionsAsync();

        Task CreatePositionAsync(Position entity);
        Task AppointMemberToPositionAsync(int mid, int pid, int sid);

        Task UpdatePositionAsync(Position entity);

        Task RemovePositionAsync(Position entity);
        Task RemovePositionByIdAsync(int id);
        Task RemoveMemberFromPositionAsync(int mid, int pid, int sid);
    }
}