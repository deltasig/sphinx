namespace DeltaSigmaPhiWebsite.Models.ViewModels
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

    public class StudyHourSubmissionModel
    {
        public StudyHour Submission { get; set; }

        public int SelectedMemberAssignmentId { get; set; }
        public IEnumerable<SelectListItem> MemberAssignments { get; set; }
        public int SelectedApproverId { get; set; }
        public IEnumerable<SelectListItem> Approvers { get; set; }
    }

    public class StudyHourIndexModel
    {
        public IEnumerable<StudyHourAssignment> StudyHourAssignments { get; set; }

        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}