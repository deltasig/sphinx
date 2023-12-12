using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ServiceHourAmendment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SemesterId { get; set; }

    public double AmountHours { get; set; }

    public string Reason { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual User User { get; set; }
}
