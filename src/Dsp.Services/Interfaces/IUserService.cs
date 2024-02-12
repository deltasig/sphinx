namespace Dsp.Services.Interfaces;

using Data.Entities;
using System.Threading.Tasks;

public interface IUserService : IService
{
    Task<User> GetUserByIdAsync(int id);
    Task<User> GetUserByUserNameAsync(string userName);
}