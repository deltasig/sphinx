namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class SoberDriversRepository : GenericRepository<SoberDriver>, ISoberDriversRepository
    {
        public SoberDriversRepository(DspContext context) : base(context)
        {
        }
    }
}