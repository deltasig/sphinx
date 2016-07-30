namespace Dsp.Services.Admin
{
    using Data;
    using Data.Entities;
    using Interfaces;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class PositionService : BaseService, IPositionService
    {
        public PositionService(SphinxDbContext db) : base(db)
        {
        }

        public async Task<Position> GetPositionByIdAsync(int id)
        {
            return await _db.Roles.SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Position> GetPositionByNameAsync(string name)
        {
            return await _db.Roles.SingleOrDefaultAsync(m => m.Name == name);
        }

        public async Task AppointMemberToPositionAsync(int mid, int pid, int sid)
        {
            _db.Leaders.Add(new Leader
            {
                UserId = mid,
                RoleId = pid,
                SemesterId = sid,
                AppointedOn = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveMemberFromPositionAsync(int mid, int pid, int sid)
        {
            var appointment = await _db.Leaders
                .SingleAsync(l => l.UserId == mid && l.RoleId == pid && l.SemesterId == sid);
            _db.Entry(appointment).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }
    }
}
