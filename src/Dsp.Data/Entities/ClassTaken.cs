using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ClassTaken
{
    public int ClassTakenId { get; set; }

    public int UserId { get; set; }

    public int ClassId { get; set; }

    public int SemesterId { get; set; }

    public bool IsSummerClass { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual Class Class { get; set; }

    public virtual Semester Semester { get; set; }

    public virtual Member User { get; set; }
}
