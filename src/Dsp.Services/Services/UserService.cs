namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class UserService : BaseService, IUserService
{
    private readonly DspDbContext _context;

    public UserService(DspDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _context.FindAsync<User>(id);
    }

    public async Task<User> GetUserByUserNameAsync(string userName)
    {
        return await _context.Users
            .Where(m => m.UserName == userName)
            .Include(m => m.MemberInfo)
            .SingleAsync();
    }
}
