namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class IncidentReport
    {
        [Key]
        public int IncidentId { get; set; }

        [Display(Name = "Submitted On")]
        public DateTime DateTimeSubmitted { get; set; }

        public int ReportedBy { get; set; }

        [Required]
        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]
        public DateTime DateTimeOfIncident { get; set; }

        [Required]
        [Display(Name = "Policy Broken")]
        [DataType(DataType.Text)]
        [StringLength(100)]
        public string PolicyBroken { get; set; }

        [Required]
        [Display(Name = "Incident Description")]
        [DataType(DataType.MultilineText)]
        [StringLength(1500)]
        public string Description { get; set; }

        [StringLength(1500)]
        [DataType(DataType.MultilineText)]
        public string OfficialReport { get; set; }

        public virtual Member Member { get; set; }
    }
}
