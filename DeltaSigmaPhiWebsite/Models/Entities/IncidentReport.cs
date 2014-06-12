namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class IncidentReport
    {
        [Key]
        public int IncidentId { get; set; }

        public DateTime DateTimeSubmitted { get; set; }

        public int ReportedBy { get; set; }

        public DateTime DateTimeOfIncident { get; set; }

        [Required]
        public string BehaviorsWitnessed { get; set; }

        [Required]
        public string PolicyBroken { get; set; }

        public DateTime? DateOfHearing { get; set; }

        public string DecisionRendered { get; set; }

        public virtual Member Member { get; set; }

        public virtual ICollection<Member> MembersInvolved { get; set; }
    }
}
