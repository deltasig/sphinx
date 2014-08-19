namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class ClassesTakenRepository : GenericRepository<ClassTaken>, IClassesTakenRepository
    {
        public ClassesTakenRepository(DspContext context) : base(context)
        {
        }
    }
}