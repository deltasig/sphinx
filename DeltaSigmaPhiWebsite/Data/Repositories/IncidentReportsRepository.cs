namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class IncidentReportsRepository : GenericRepository<IncidentReport>, IIncidentReportsRepository
    {
        public IncidentReportsRepository(DspContext context) : base(context)
        {
        }
    }
}