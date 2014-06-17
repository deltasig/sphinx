namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class LeadersRepository : GenericRepository<Leader>, ILeadersRepository
    {
        public LeadersRepository(DspContext context) : base(context)
        {

        }
    }
}