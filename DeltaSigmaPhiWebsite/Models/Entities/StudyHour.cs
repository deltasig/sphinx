namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class StudyHour
    {
        [Key]
        [Column(Order = 0)]
        public int StudyHourId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SubmittedBy { get; set; }

        [Key]
        [Column(Order = 2)]
        [DataType(DataType.Date)]
        [Display(Name = "Studied On")]
        public DateTime DateTimeStudied { get; set; }

        public int? ApproverId { get; set; }
        
        [Required]
        [Display(Name = "Durations (Hours)")]
        public double DurationHours { get; set; }

        [Required]
        [Display(Name = "Proctored?")]
        public bool IsProctored { get; set; }

        [Display(Name = "Submitted On")]
        public DateTime DateTimeSubmitted { get; set; }

        [Display(Name = "Approved On")]
        public DateTime? DateTimeApproved { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Length of Event name must be 1-50 characters.")]
        [Display(Name = "Location")]
        public string StudyLocation { get; set; }

        public int RequiredStudyHours { get; set; }
        public int? ProctoredStudyHours { get; set; }

        public virtual Member Submitter { get; set; }
        public virtual Member Approver { get; set; }
    }
}
