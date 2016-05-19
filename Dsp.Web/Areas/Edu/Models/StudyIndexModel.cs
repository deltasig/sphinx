namespace Dsp.Web.Areas.Edu.Models
{
    using System.Collections.Generic;
    using Entities;

    public class StudyIndexModel
    {
        public IEnumerable<StudyPeriod> Periods { get; set; }
        public IEnumerable<StudySession> Sessions { get; set; }
        public Semester Semester { get; set; }
    }
}
