using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipQuestion
{
    public int ScholarshipQuestionId { get; set; }

    public string Prompt { get; set; }

    public int AnswerMinimumLength { get; set; }

    public int AnswerMaximumLength { get; set; }

    public virtual ICollection<ScholarshipAnswer> Answers { get; set; } = new List<ScholarshipAnswer>();

    public virtual ICollection<ScholarshipAppQuestion> Questions { get; set; } = new List<ScholarshipAppQuestion>();
}
