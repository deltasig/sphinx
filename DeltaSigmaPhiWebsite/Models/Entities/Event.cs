namespace DeltaSigmaPhiWebsite.Models.Entities
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;

	public partial class Event
	{
		public Event()
		{
			ServiceHours = new HashSet<ServiceHour>();
		}

		public int EventId { get; set; }

		[Required]
		[Display(Name = "Date/Time of Event")]
		[DisplayFormat(DataFormatString = "{0:MM-dd-yyyy hh:mm tt}", ApplyFormatInEditMode = true)]
		public DateTime DateTimeOccurred { get; set; }

		[Required]
		[Display(Name = "Event Name")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Length of Event name must be 1-50 characters.")]
		public string EventName { get; set; }

		[Required]
		[Display(Name = "Event Duration (Hrs)")]
        [DataType(DataType.Duration)]
        [Range(0.5, 1000, ErrorMessage = "Please enter a number from 0-1000")]
		public double DurationHours { get; set; }

		public virtual ICollection<ServiceHour> ServiceHours { get; set; }
	}
}
