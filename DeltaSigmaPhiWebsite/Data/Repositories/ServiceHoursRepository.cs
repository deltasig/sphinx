namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class ServiceHoursRepository : GenericRepository<ServiceHour>, IServiceHoursRepository
    {
        public ServiceHoursRepository(DspContext context) : base(context)
        {
        }
    }
}