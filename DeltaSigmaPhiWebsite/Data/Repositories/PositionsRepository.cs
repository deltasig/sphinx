namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class PositionsRepository : GenericRepository<Position>, IPositionsRepository
    {
        public PositionsRepository(DspContext context) : base(context)
        {

        }
    }
}