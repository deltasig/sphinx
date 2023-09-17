using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();
}
