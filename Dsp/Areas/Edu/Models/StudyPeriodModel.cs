namespace Dsp.Areas.Edu.Models
{
    using System.Collections.Generic;
    using Entities;

    public class StudyPeriodModel
    {
        public StudyPeriod StudyPeriod { get; set; }
        public double DefaultHourAmount { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<StudySession> StudySessions { get; set; }
        public int? PreviousStudyPeriodId { get; set; }
    }
}
