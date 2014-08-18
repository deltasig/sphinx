namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class SoberSignupsRepository : GenericRepository<SoberSignup>, ISoberSignupsRepository
    {
        public SoberSignupsRepository(DspContext context) : base(context)
        {
        }
    }
}