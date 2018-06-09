namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILaundryService
    {
        Task<IEnumerable<LaundrySignup>> GetSignups(DateTime beginningOn);

        Task Reserve(LaundrySignup entity, int maxSignupsAllowed = 2);

        Task Cancel(LaundrySignup entity);
    }
}