namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class StudyHour
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudyHourId { get; set; }

        [Column(Order = 1)]
        [DataType(DataType.Date)]
        [Index("IX_DateTimeAssignment", 1, IsUnique = true)]
        public DateTime DateTimeStudied { get; set; }

        [Column(Order = 2)]
        [Index("IX_DateTimeAssignment", 2, IsUnique = true)]
        public int AssignmentId { get; set; }
        
        [Required]
        [Range(0.5, 12, ErrorMessage = "Duration must be between 0.5 and 6.")]
        public double DurationHours { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Location text must be 1-50 characters.")]
        public string StudyLocation { get; set; }

        [Required]
        public int ApproverId { get; set; }

        [Required]
        public bool IsProctored { get; set; }

        public DateTime DateTimeSubmitted { get; set; }

        public DateTime? DateTimeApproved { get; set; }

        [ForeignKey("ApproverId")]
        public virtual Member Approver { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual StudyAssignment Assignment { get; set; }
    }
}
