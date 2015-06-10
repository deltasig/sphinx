namespace Dsp.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Event
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        public int? SubmitterId { get; set; }

        [Required, Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Required, Display(Name = "Date/Time of Event")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
        public DateTime DateTimeOccurred { get; set; }

        [Required, DataType(DataType.Text), Display(Name = "Event Name")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Length of Event name must be 1-50 characters.")]
        public string EventName { get; set; }

        [Required, DataType(DataType.Duration), Display(Name = "Event Duration (Hrs)")]
        [Range(0.5, 1000, ErrorMessage = "Please enter a number from 0-1000")]
        public double DurationHours { get; set; }
        
        [ForeignKey("SubmitterId")]
        public virtual Member Submitter { get; set; }
        [InverseProperty("Event")]
        public virtual ICollection<ServiceHour> ServiceHours { get; set; }
    }
}
