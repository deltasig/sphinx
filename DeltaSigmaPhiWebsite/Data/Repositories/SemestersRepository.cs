namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class SemestersRepository : GenericRepository<Semester>, ISemestersRepository
    {
        public SemestersRepository(DspContext context) : base(context)
        {
        }
    }
}