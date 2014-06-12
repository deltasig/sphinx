namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ServiceHour
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EventId { get; set; }

        public double DurationHours { get; set; }

        public DateTime DateTimeSubmitted { get; set; }

        public virtual Event Event { get; set; }

        public virtual Member Member { get; set; }
    }
}
