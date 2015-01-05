namespace DeltaSigmaPhiWebsite.Areas.Study.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Entities;

    public class StudyPeriodIndexModel
    {
        public IEnumerable<StudyPeriod> Periods { get; set; }

        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}