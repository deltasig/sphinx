namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ChoresRepository : GenericRepository<Chore>, IChoresRepository
    {
        public ChoresRepository(DspContext context) : base(context)
        {
        }
    }
}