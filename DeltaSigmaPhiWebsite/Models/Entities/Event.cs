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
		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}", ApplyFormatInEditMode = true)]
		public DateTime DateTimeOccurred { get; set; }

		[Required]
		[Display(Name = "Event Name")]
		[DataType(DataType.Text)]
		[StringLength(50)]
		public string EventName { get; set; }

		[Required]
		[Display(Name = "Event Duration (Hrs)")]
		[DataType(DataType.Duration)]
		public double DurationHours { get; set; }

		public virtual ICollection<ServiceHour> ServiceHours { get; set; }
	}
}
