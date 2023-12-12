using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipAppQuestion
{
    public int ScholarshipAppQuestionId { get; set; }

    public int ScholarshipAppId { get; set; }

    public int ScholarshipQuestionId { get; set; }

    public int FormOrder { get; set; }

    public bool IsOptional { get; set; }

    public virtual ScholarshipApp Application { get; set; }

    public virtual ScholarshipQuestion Question { get; set; }
}
