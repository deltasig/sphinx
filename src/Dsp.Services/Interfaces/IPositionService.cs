namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Threading.Tasks;

    public interface IPositionService
    {
        Task<Position> GetPositionByIdAsync(int id);
        Task<Position> GetPositionByNameAsync(string name);
        Task AppointMemberToPositionAsync(int mid, int pid, int sid);
        Task RemoveMemberFromPositionAsync(int mid, int pid, int sid);
    }
}