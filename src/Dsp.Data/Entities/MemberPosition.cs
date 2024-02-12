using System;

namespace Dsp.Data.Entities;

public partial class MemberPosition
{
    public virtual int UserId { get; set; } = default!;

    public virtual int RoleId { get; set; } = default!;

    public int SemesterId { get; set; }

    public DateTime AppointedOn { get; set; }

    public virtual Position Role { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual Member User { get; set; }
}
