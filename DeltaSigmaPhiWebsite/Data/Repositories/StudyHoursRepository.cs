namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class StudyHoursRepository : GenericRepository<StudyHour>, IStudyHoursRepository
    {
        public StudyHoursRepository(DspContext context) : base(context)
        {
        }
    }
}