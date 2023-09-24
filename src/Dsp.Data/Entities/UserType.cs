using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class UserType
{
    public int StatusId { get; set; }

    public string StatusName { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
