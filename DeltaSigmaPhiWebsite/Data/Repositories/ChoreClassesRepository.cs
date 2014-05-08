namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ChoreClassesRepository : GenericRepository<ChoreClass>, IChoreClassesRepository
    {
        public ChoreClassesRepository(DspContext context) : base(context)
        {
        }
    }
}