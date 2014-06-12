namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class StudyHoursRepository : GenericRepository<StudyHour>, IStudyHoursRepository
    {
        public StudyHoursRepository(DspContext context) : base(context)
        {
        }
    }
}