namespace Dsp.Areas.Edu.Models
{
    using System.Collections.Generic;
    using Entities;

    public class StudySessionModel
    {
        public StudySession StudySession { get; set; }
        public StudyPeriod StudyPeriod { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<StudySession> StudySessions { get; set; }
        public int? PreviousStudySessionId { get; set; }
    }
}
