namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ChoreGroupTypesRepository : GenericRepository<ChoreGroupType>, IChoreGroupTypesRepository
    {
        public ChoreGroupTypesRepository(DspContext context) : base(context)
        {
        }
    }
}