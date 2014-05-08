namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class IncidentReportsRepository : GenericRepository<IncidentReport>, IIncidentReportsRepository
    {
        public IncidentReportsRepository(DspContext context) : base(context)
        {
        }
    }
}