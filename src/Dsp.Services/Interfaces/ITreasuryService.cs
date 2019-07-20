namespace Dsp.Services.Interfaces
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITreasuryService : IService
    {
        Task<Cause> GetCauseByIdAsync(int id);
        Task<IEnumerable<Cause>> GetAllCausesAsync();
        Task<Fundraiser> GetFundraiserByIdAsync(int id);
        Task<IEnumerable<Fundraiser>> GetAllFundraisersAsync();
        Task<IEnumerable<Fundraiser>> GetActiveFundraisersAsync();
        Task<IEnumerable<Fundraiser>> GetActivePledgeableFundraisersAsync();
        Task<Donation> GetDonationByIdAsync(int id);

        Task CreateCauseAsync(Cause signup);
        Task CreateFundraiserAsync(Fundraiser signups);
        Task CreateDonationAsync(Donation type);

        Task UpdateCauseAsync(Cause signup);
        Task UpdateFundraiserAsync(Fundraiser type);
        Task UpdateDonationAsync(Donation type);

        Task DeleteCauseAsync(int id);
        Task DeleteFundraiserAsync(int id);
        Task DeleteDonationAsync(int id);
    }
}