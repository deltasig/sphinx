namespace DeltaSigmaPhiWebsite.Areas.Sphinx.Models
{
    using Entities;
    using System.Collections.Generic;

    public class SphinxHomeIndexModel
    {
        public Member MemberInfo { get; set; }
        public bool NeedsToSoberDrive { get; set; }
        public Semester CurrentSemester { get; set; }
        public Semester PreviousSemester { get; set; }
        public double RemainingCommunityServiceHours { get; set; }
        public IEnumerable<LaundrySignup> LaundrySummary { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<ServiceHour> CompletedEvents { get; set; }
        public IEnumerable<SoberSignup> SoberSignups { get; set; }
        public IEnumerable<MemberStudyHourAssignment> MemberStudyHourAssignments { get; set; }
        public IEnumerable<StudyHour> ApprovalRequests { get; set; }
    }

    
}

