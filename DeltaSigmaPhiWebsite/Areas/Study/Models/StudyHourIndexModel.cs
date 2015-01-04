namespace DeltaSigmaPhiWebsite.Areas.Study.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Entities;

    public class StudyHourIndexModel
    {
        public IEnumerable<StudyHourAssignment> StudyHourAssignments { get; set; }

        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}