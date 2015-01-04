namespace DeltaSigmaPhiWebsite.Areas.Study.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class StudyHourAssignmentInfoModel
    {
        public StudyHourAssignment Assignment { get; set; }
        public MemberStudyHourAssignment MemberAssignment { get; set; }
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}