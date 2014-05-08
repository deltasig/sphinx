namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class SemestersRepository : GenericRepository<Semester>, ISemestersRepository
    {
        public SemestersRepository(DspContext context) : base(context)
        {
        }
    }
}