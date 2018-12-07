namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class StatusService : BaseService, IStatusService
    {
        private readonly IRepository _repository;

        public StatusService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public StatusService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public StatusService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MemberStatus>> GetAllStatusesAsync()
        {
            var statuses = await _repository
                .GetAllAsync<MemberStatus>(
                    orderBy: o => o.OrderBy(s => s.StatusId));
            return statuses;
        }

        public async Task<MemberStatus> GetStatusByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<MemberStatus>(id);
        }

        public async Task CreateStatus(MemberStatus status)
        {
            _repository.Create(status);
            await _repository.SaveAsync();
        }

        public async Task UpdateStatus(MemberStatus status)
        {
            _repository.Update(status);
            await _repository.SaveAsync();
        }

        public async Task DeleteStatus(int id)
        {
            _repository.Delete<MemberStatus>(id);
            await _repository.SaveAsync();
        }
    }
}
