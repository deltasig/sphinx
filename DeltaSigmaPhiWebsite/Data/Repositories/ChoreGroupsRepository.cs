namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ChoreGroupsRepository : GenericRepository<ChoreGroup>, IChoreGroupsRepository
    {
        public ChoreGroupsRepository(DspContext context) : base(context)
        {
        }
    }
}