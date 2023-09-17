using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Semester
{
    public int Id { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public DateTime TransitionDate { get; set; }

    public int MinimumServiceHours { get; set; }

    public int MinimumServiceEvents { get; set; }

    public string RecruitmentBookUrl { get; set; }

    public virtual ICollection<ClassTaken> ClassesTaken { get; set; } = new List<ClassTaken>();

    public virtual ICollection<Leader> Leaders { get; set; } = new List<Leader>();

    public virtual ICollection<Member> GraduatingMembers { get; set; } = new List<Member>();

    public virtual ICollection<PledgeClass> PledgeClasses { get; set; } = new List<PledgeClass>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<ServiceEventAmendment> ServiceEventAmendments { get; set; } = new List<ServiceEventAmendment>();

    public virtual ICollection<ServiceHourAmendment> ServiceHourAmendments { get; set; } = new List<ServiceHourAmendment>();

    public virtual ICollection<ServiceEvent> ServiceEvents { get; set; } = new List<ServiceEvent>();

    public override string ToString()
    {
        return (DateStart.Month < 6 ? "Spring " : "Fall ") + DateStart.Year;
    }
}
