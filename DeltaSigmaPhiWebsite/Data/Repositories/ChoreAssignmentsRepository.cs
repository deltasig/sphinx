namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ChoreAssignmentsRepository : GenericRepository<ChoreAssignment>, IChoreAssignmentsRepository
    {
        public ChoreAssignmentsRepository(DspContext context) : base(context)
        {
        }
    }
}