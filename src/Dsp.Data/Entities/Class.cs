using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Class
{
    public int ClassId { get; set; }

    public int DepartmentId { get; set; }

    public string CourseShorthand { get; set; }

    public string CourseName { get; set; }

    public int CreditHours { get; set; }

    public virtual ICollection<ClassTaken> ClassesTaken { get; set; } = new List<ClassTaken>();

    public virtual Department Department { get; set; }
}
