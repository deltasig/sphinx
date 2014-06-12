namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class SoberDriversRepository : GenericRepository<SoberDriver>, ISoberDriversRepository
    {
        public SoberDriversRepository(DspContext context) : base(context)
        {
        }
    }
}