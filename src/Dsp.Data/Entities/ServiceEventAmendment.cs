using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ServiceEventAmendment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SemesterId { get; set; }

    public string Reason { get; set; }

    public int NumberEvents { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual Member User { get; set; }
}
