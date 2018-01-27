namespace Dsp.Services
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Dsp.Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TreasuryService : BaseService, ITreasuryService
    {
        private readonly IRepository _repository;

        public TreasuryService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public TreasuryService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public TreasuryService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Donation> GetDonationByIdAsync(int id)
        {
            var donation = await _repository.GetByIdAsync<Donation>(id);
            donation.CreatedOn = base.ConvertUtcToCst(donation.CreatedOn);
            if (donation.ReceivedOn != null)
            {
                donation.ReceivedOn = base.ConvertUtcToCst(donation.ReceivedOn.Value);
            }
            return donation;
        }

        public async Task<Fundraiser> GetFundraiserByIdAsync(int id)
        {
            var fundraiser = await _repository.GetByIdAsync<Fundraiser>(id);
            fundraiser.BeginsOn = base.ConvertUtcToCst(fundraiser.BeginsOn);
            if (fundraiser.EndsOn != null)
            {
                fundraiser.EndsOn = base.ConvertUtcToCst(fundraiser.EndsOn.Value);
            }
            return fundraiser;
        }

        public async Task<Cause> GetCauseByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<Cause>(id);
        }

        public async Task<IEnumerable<Cause>> GetAllCausesAsync()
        {
            return await _repository.GetAllAsync<Cause>(o => o.OrderBy(p => p.Name));
        }

        public async Task<IEnumerable<Fundraiser>> GetAllFundraisersAsync()
        {
            return await _repository.GetAllAsync<Fundraiser>(o => o.OrderBy(p => p.Name));
        }

        public async Task<IEnumerable<Fundraiser>> GetActiveFundraisersAsync()
        {
            return await _repository.GetAsync<Fundraiser>(
                filter: m => m.IsPublic && (m.EndsOn == null || m.EndsOn > DateTime.UtcNow),
                orderBy: o => o.OrderByDescending(p => p.EndsOn).ThenBy(p => p.Name));
        }

        public async Task<IEnumerable<Fundraiser>> GetActivePledgeableFundraisersAsync()
        {
            return await _repository.GetAsync<Fundraiser>(
                filter: m => m.IsPledgeable && m.IsPublic && (m.EndsOn == null || m.EndsOn > DateTime.UtcNow),
                orderBy: o => o.OrderByDescending(p => p.EndsOn).ThenBy(p => p.Name));
        }

        public async Task AddDonationAsync(Donation donation)
        {
            donation.CreatedOn = DateTime.UtcNow;
            if (donation.ReceivedOn != null)
            {
                donation.ReceivedOn = base.ConvertCstToUtc(donation.ReceivedOn.Value);
            }
            _repository.Create(donation);
            await _repository.SaveAsync();
        }

        public async Task AddFundraiserAsync(Fundraiser fundraiser)
        {
            _repository.Create(fundraiser);
            await _repository.SaveAsync();
        }

        public async Task AddCauseAsync(Cause cause)
        {
            _repository.Create(cause);
            await _repository.SaveAsync();
        }

        public async Task UpdateDonationAsync(Donation donation)
        {
            if (donation.ReceivedOn != null)
            {
                donation.ReceivedOn = base.ConvertCstToUtc(donation.ReceivedOn.Value);
            }
            _repository.Update(donation);
            await _repository.SaveAsync();
        }

        public async Task UpdateFundraiserAsync(Fundraiser fundraiser)
        {
            fundraiser.BeginsOn = base.ConvertCstToUtc(fundraiser.BeginsOn);
            if (fundraiser.EndsOn != null)
            {
                fundraiser.EndsOn = base.ConvertCstToUtc(fundraiser.EndsOn.Value);
            }
            _repository.Update(fundraiser);
            await _repository.SaveAsync();
        }

        public async Task UpdateCauseAsync(Cause cause)
        {
            _repository.Update(cause);
            await _repository.SaveAsync();
        }

        public async Task DeleteDonationAsync(int id)
        {
            _repository.Delete<Donation>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteFundraiserAsync(int id)
        {
            _repository.Delete<Fundraiser>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteCauseAsync(int id)
        {
            _repository.Delete<Cause>(id);
            await _repository.SaveAsync();
        }
    }
}