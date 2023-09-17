using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipApp
{
    public int ScholarshipAppId { get; set; }

    public int ScholarshipTypeId { get; set; }

    public string Title { get; set; }

    public string AdditionalText { get; set; }

    public DateTime OpensOn { get; set; }

    public DateTime ClosesOn { get; set; }

    public bool IsPublic { get; set; }

    public virtual ICollection<ScholarshipAppQuestion> Questions { get; set; } = new List<ScholarshipAppQuestion>();

    public virtual ICollection<ScholarshipSubmission> Submissions { get; set; } = new List<ScholarshipSubmission>();

    public virtual ScholarshipType Type { get; set; }
}
