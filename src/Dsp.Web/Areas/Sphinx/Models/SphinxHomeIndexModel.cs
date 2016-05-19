namespace Dsp.Web.Areas.Sphinx.Models
{
    using Dsp.Data.Entities;
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

        // Alumni/Admin info
        public int DaysSinceIncident { get; set; }
        public int IncidentsThisSemester { get; set; }
        public int ScholarshipSubmissionsThisYear { get; set; }
        public int LaundryUsageThisSemester { get; set; }
        public double ServiceHoursThisSemester { get; set; }
        public int NewMembersThisSemester { get; set; }
    }
}

