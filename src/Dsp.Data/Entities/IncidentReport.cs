namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class IncidentReport
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Submitted On")]
        public DateTime DateTimeSubmitted { get; set; }

        public int ReportedBy { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Date of Incident")]
        public DateTime DateTimeOfIncident { get; set; }

        [Required, DataType(DataType.Text), StringLength(100), Display(Name = "Policy Broken")]
        public string PolicyBroken { get; set; }

        [Required, DataType(DataType.MultilineText), StringLength(1500), Display(Name = "Incident Description")]
        public string Description { get; set; }

        [Required, DataType(DataType.MultilineText), StringLength(3000), Display(Name = "Investigation Notes")]
        public string InvestigationNotes { get; set; }

        [DataType(DataType.MultilineText), StringLength(1500), Display(Name = "Official Report")]
        public string OfficialReport { get; set; }

        [ForeignKey("ReportedBy")]
        public virtual Member Member { get; set; }
    }
}
