using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ServiceEvent
{
    public int EventId { get; set; }

    public int? SubmitterId { get; set; }

    public bool IsApproved { get; set; }

    public DateTime DateTimeOccurred { get; set; }

    public string EventName { get; set; }

    public double DurationHours { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int SemesterId { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual ICollection<ServiceHour> ServiceHours { get; set; } = new List<ServiceHour>();

    public virtual User Submitter { get; set; }
}
