using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MajorToMember
{
    public int MajorToMemberId { get; set; }

    public int MajorId { get; set; }

    public int UserId { get; set; }

    public DegreeLevel DegreeLevel { get; set; }

    public virtual Major Major { get; set; }

    public virtual User User { get; set; }
}

public enum DegreeLevel
{
    Bs,
    Minor,
    Ms,
    PhD,
    Cert,
    Ba
}
