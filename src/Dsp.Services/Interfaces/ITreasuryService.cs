namespace Dsp.Services.Interfaces
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITreasuryService
    {
        Task<Cause> GetCauseByIdAsync(int id);
        Task<IEnumerable<Cause>> GetAllCausesAsync();
        Task<Fundraiser> GetFundraiserByIdAsync(int id);
        Task<IEnumerable<Fundraiser>> GetAllFundraisersAsync();
        Task<Donation> GetDonationByIdAsync(int id);

        Task AddCauseAsync(Cause signup);
        Task AddFundraiserAsync(Fundraiser signups);
        Task AddDonationAsync(Donation type);

        Task UpdateCauseAsync(Cause signup);
        Task UpdateFundraiserAsync(Fundraiser type);
        Task UpdateDonationAsync(Donation type);

        Task DeleteCauseAsync(int id);
        Task DeleteFundraiserAsync(int id);
        Task DeleteDonationAsync(int id);
    }
}