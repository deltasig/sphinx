namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class PledgeClassesRepository : GenericRepository<PledgeClass>, IPledgeClassesRepository
    {
        public PledgeClassesRepository(DspContext context) : base(context)
        {
        }
    }
}