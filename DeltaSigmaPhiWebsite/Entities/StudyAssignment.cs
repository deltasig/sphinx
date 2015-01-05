namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class StudyAssignment
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudyAssignmentId { get; set; }

        [Column(Order = 1)]
        [Index("IX_AssignedMemberPeriodAmount", 1, IsUnique = true)]
        public int AssignedMemberId { get; set; }

        [Column(Order = 2)]
        [Index("IX_AssignedMemberPeriodAmount", 2, IsUnique = true)]
        public int PeriodId { get; set; }

        [Required]
        [Column(Order = 3)]
        [Index("IX_AssignedMemberPeriodAmount", 3, IsUnique = true)]
        [Range(0.5, 100, ErrorMessage = "Duration must be between 0.5 and 100.")]
        public double UnproctoredAmount { get; set; }

        [Required]
        [Column(Order = 4)]
        [Index("IX_AssignedMemberPeriodAmount", 4, IsUnique = true)]
        [Range(0.0, 100, ErrorMessage = "Duration must be between 0.0 and 100.")]
        public double ProctoredAmount { get; set; }

        public DateTime AssignedOn { get; set; }

        [ForeignKey("AssignedMemberId")]
        public virtual Member AssignedMember { get; set; }

        [ForeignKey("PeriodId")]
        public virtual StudyPeriod Period { get; set; }

        public virtual ICollection<StudyHour> TurnIns { get; set; }
    }
}