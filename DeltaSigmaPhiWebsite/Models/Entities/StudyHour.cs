namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class StudyHour
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTimeStudied { get; set; }

        public double DurationHours { get; set; }

        public DateTime DateTimeSubmitted { get; set; }

        public int? DesignatedApprover { get; set; }

        public bool? IsApproved { get; set; }

        public DateTime? DateTimeApproved { get; set; }

        public bool? IsProctored { get; set; }

        public virtual Member Member { get; set; }
    }
}
