using Microsoft.AspNetCore.Identity;
using System;

namespace Dsp.Data.Entities;

public partial class UserRole : IdentityUserRole<int>
{
    public int SemesterId { get; set; }

    public DateTime AppointedOn { get; set; }

    public virtual Role Role { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual User User { get; set; }
}
