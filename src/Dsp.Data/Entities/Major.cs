using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Major
{
    public int MajorId { get; set; }

    public int DepartmentId { get; set; }

    public string MajorName { get; set; }

    public virtual Department Department { get; set; }

    public virtual ICollection<MajorToMember> MajorToMembers { get; set; } = new List<MajorToMember>();
}
