namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class StudyPeriod
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PeriodId { get; set; }

        [Required]
        [Column(Order = 1)]
        [DataType(DataType.Date)]
        [Index("IX_PeriodDateTime", 1, IsUnique = true)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [Required]
        [Column(Order = 2)]
        [DataType(DataType.Date)]
        [Index("IX_PeriodDateTime", 2, IsUnique = true)]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime End { get; set; }
        
        public virtual ICollection<StudyAssignment> Assignments { get; set; }
    }
}