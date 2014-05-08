namespace DeltaSigmaPhiWebsite.Data.Interfaces
{
    using System;
    using Models;

    public interface ILaundrySignupRepository : IGenericRepository<LaundrySignup>
    {
        void DeleteByShift(DateTime dateTime);
    }
}