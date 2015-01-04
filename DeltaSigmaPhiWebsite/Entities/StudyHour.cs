namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
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
        [Index("IX_SubmitterAndDateTimeStudiedAndAssignment", 1, IsUnique = true)]
        public DateTime DateTimeStudied { get; set; }

        [Column(Order = 2)]
        [Index("IX_SubmitterAndDateTimeStudiedAndAssignment", 2, IsUnique = true)]
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
        public virtual MemberStudyHourAssignment MemberAssignment { get; set; }
    }

    public partial class StudyHourAssignment
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudyHourAssignmentId { get; set; }

        [Required]
        [Column(Order = 1)]
        [DataType(DataType.Date)]
        [Index("IX_StudyHourAssignmentDateTimeAmount", 1, IsUnique = true)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [Required]
        [Column(Order = 2)]
        [DataType(DataType.Date)]
        [Index("IX_StudyHourAssignmentDateTimeAmount", 2, IsUnique = true)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime End { get; set; }
        
        public virtual ICollection<MemberStudyHourAssignment> MembersAssigned { get; set; }
    }

    public partial class MemberStudyHourAssignment
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemberStudyHourAssignmentId { get; set; }

        [Column(Order = 1)]
        [Index("IX_AssignedMemberAndStartAndEndAmount", 1, IsUnique = true)]
        public int AssignedMemberId { get; set; }

        [Column(Order = 2)]
        [Index("IX_AssignedMemberAndStartAndEndAmount", 2, IsUnique = true)]
        public int AssignmentId { get; set; }

        [Required]
        [Column(Order = 3)]
        [Index("IX_AssignedMemberAndStartAndEndAmount", 3, IsUnique = true)]
        [Range(0.5, 100, ErrorMessage = "Duration must be between 0.5 and 100.")]
        public double UnproctoredAmount { get; set; }

        [Required]
        [Column(Order = 4)]
        [Index("IX_AssignedMemberAndStartAndEndAmount", 4, IsUnique = true)]
        [Range(0.5, 100, ErrorMessage = "Duration must be between 0.5 and 100.")]
        public double ProctoredAmount { get; set; }

        public DateTime AssignedOn { get; set; }

        [ForeignKey("AssignedMemberId")]
        public virtual Member AssignedMember { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual StudyHourAssignment Assignment { get; set; }

        public virtual ICollection<StudyHour> TurnIns { get; set; }
    }
}
