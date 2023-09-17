namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class StatusService : BaseService, IStatusService
    {
        private readonly DspDbContext _context;

        public StatusService(DspDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MemberStatus>> GetAllStatusesAsync()
        {
            var statuses = await _context.MemberStatuses
                .OrderBy(s => s.StatusId)
                .ToListAsync();
            return statuses;
        }

        public async Task<MemberStatus> GetStatusByIdAsync(int id)
        {
            return await _context.FindAsync<MemberStatus>(id);
        }

        public async Task CreateStatus(MemberStatus status)
        {
            _context.Add(status);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatus(MemberStatus status)
        {
            _context.Update(status);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStatus(int id)
        {
            var entity = new MemberStatus { StatusId = id };
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
