using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipType
{
    public int ScholarshipTypeId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<ScholarshipApp> Applications { get; set; } = new List<ScholarshipApp>();
}
