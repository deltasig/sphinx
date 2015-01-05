namespace DeltaSigmaPhiWebsite.Areas.Study.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Entities;

    public class StudyHourSubmissionModel
    {
        public StudyHour Submission { get; set; }

        public IEnumerable<SelectListItem> MemberAssignments { get; set; }
        public IEnumerable<SelectListItem> Approvers { get; set; }
    }
}