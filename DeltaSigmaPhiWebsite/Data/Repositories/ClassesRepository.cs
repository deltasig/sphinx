namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class ClassesRepository : GenericRepository<Class>, IClassesRepository
    {
        public ClassesRepository(DspContext context) : base(context)
        {
        }
    }
}