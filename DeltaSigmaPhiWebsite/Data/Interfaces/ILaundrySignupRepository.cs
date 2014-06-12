namespace DeltaSigmaPhiWebsite.Data.Interfaces
{
    using Models.Entities;
    using System;

    public interface ILaundrySignupRepository : IGenericRepository<LaundrySignup>
    {
        void DeleteByShift(DateTime dateTime);
    }
}