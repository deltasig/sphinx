namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStatusService : IService
{
    Task<IEnumerable<UserType>> GetAllStatusesAsync();
    Task<UserType> GetStatusByIdAsync(int id);
    Task CreateStatus(UserType status);
    Task UpdateStatus(UserType status);
    Task DeleteStatus(int id);
}