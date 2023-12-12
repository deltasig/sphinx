using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Room
{
    public int RoomId { get; set; }

    public string Name { get; set; }

    public int MaxCapacity { get; set; }

    public int SemesterId { get; set; }

    public virtual ICollection<RoomToMember> Members { get; set; } = new List<RoomToMember>();

    public virtual Semester Semester { get; set; }
}
