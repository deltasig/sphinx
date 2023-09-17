using Microsoft.AspNetCore.Identity;
using System;

namespace Dsp.Data.Entities;

public partial class Leader : IdentityUserRole<int>
{
    public int SemesterId { get; set; }

    public DateTime AppointedOn { get; set; }

    public virtual Position Position { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual Member User { get; set; }
}
