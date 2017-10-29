namespace Dsp.Services.Interfaces
{
    using Dsp.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISoberService
    {
        Task<IEnumerable<SoberSignup>> GetUpcomingSoberSignupsAsync();

        Task<IEnumerable<SoberSignup>> GetUpcomingSoberSignupsAsync(DateTime date);
    }
}