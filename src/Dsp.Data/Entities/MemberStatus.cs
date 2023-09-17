using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MemberStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
