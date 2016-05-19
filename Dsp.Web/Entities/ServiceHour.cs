namespace Dsp.Web.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ServiceHour
    {
        [Key, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EventId { get; set; }

        public double DurationHours { get; set; }

        public DateTime DateTimeSubmitted { get; set; }
        
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
        [ForeignKey("EventId")]
        public virtual ServiceEvent Event { get; set; }
    }
}
