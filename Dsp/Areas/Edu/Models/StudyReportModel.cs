namespace Dsp.Areas.Edu.Models
{
    using Dsp.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class StudyReportModel
    {
        public IEnumerable<SelectListItem> SemesterList { get; set; }
        public Semester Semester { get; set; }
        public List<StudyPeriod> Periods { get; set; }
        public List<StudyReportRecord> Records { get; set; }

        public StudyReportModel(Semester semester)
        {
            Semester = semester;
            Records = new List<StudyReportRecord>();
        }
    }

    public class StudyReportRecord
    {
        public Member Member { get; set; }
        public List<StudyReportPeriodRecord> Periods { get; set; }
    }

    public class StudyReportPeriodRecord
    {
        public StudyPeriod Period { get; set; }
        public StudyAssignment Assignment { get; set; }
        public double Completed { get; set; }
        public double Goal { get; set; }
        public int SessionsAttended { get; set; }
    }
}
