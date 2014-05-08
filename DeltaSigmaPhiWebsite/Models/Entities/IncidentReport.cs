namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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
