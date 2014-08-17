namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class DepartmentsRepository : GenericRepository<Department>, IDepartmentsRepository
    {
        public DepartmentsRepository(DspContext context) : base(context)
        {
        }
    }
}