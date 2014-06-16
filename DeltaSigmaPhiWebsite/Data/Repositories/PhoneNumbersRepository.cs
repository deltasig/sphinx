namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class PhoneNumbersRepository : GenericRepository<PhoneNumber>, IPhoneNumbersRepository
    {
        public PhoneNumbersRepository(DspContext context) : base(context)
        {
        }
    }
}