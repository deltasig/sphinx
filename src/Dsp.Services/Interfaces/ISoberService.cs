namespace Dsp.Services.Interfaces
{
    using Dsp.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISoberService
    {
        Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync();
        Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync(DateTime date);
        Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync();
        Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync(DateTime start);
        Task<IEnumerable<SoberSignup>> GetFutureVacantSignups();
        Task<IEnumerable<SoberType>> GetTypesAsync();

        Task CreateSignupAsync(SoberSignup signup);
        Task CreateSignupsAsync(IEnumerable<SoberSignup> signups);
        Task CreateTypeAsync(SoberType type);

        Task UpdateSignupAsync(SoberSignup signup);
        Task UpdateTypeAsync(SoberType type);

        Task DeleteSignupAsync(int id);
        Task DeleteTypeAsync(int id);
    }
}