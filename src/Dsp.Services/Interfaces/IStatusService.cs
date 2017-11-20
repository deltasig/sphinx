namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IStatusService
    {
        Task<IEnumerable<MemberStatus>> GetAllStatusesAsync();
        Task<MemberStatus> GetStatusByIdAsync(int id);
        Task AddStatus(MemberStatus status);
        Task UpdateStatus(MemberStatus status);
        Task DeleteStatus(int id);
    }
}