using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class RoomToMember
{
    public int RoomToMemberId { get; set; }

    public int RoomId { get; set; }

    public int UserId { get; set; }

    public DateTime MovedIn { get; set; }

    public DateTime MovedOut { get; set; }

    public virtual Room Room { get; set; }

    public virtual Member User { get; set; }

    public string GetSemester()
    {
        if (MovedIn.Month < 6) return "Spring " + MovedIn.Year;
        else if (MovedIn.Month < 8) return "Summer " + MovedIn.Year;
        else return "Fall " + MovedIn.Year;
    }
}
