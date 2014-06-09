namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class AddressesRepository : GenericRepository<Address>, IAddressesRepository
    {
        public AddressesRepository(DspContext context) : base(context)
        {
        }
    }
}