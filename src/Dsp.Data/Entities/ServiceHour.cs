using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ServiceHour
{
    public int UserId { get; set; }

    public int EventId { get; set; }

    public double DurationHours { get; set; }

    public DateTime DateTimeSubmitted { get; set; }

    public virtual ServiceEvent Event { get; set; }

    public virtual User User { get; set; }
}
