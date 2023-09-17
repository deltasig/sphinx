using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipAnswer
{
    public int ScholarshipAnswerId { get; set; }

    public Guid ScholarshipSubmissionId { get; set; }

    public int ScholarshipQuestionId { get; set; }

    public string AnswerText { get; set; }

    public virtual ScholarshipQuestion Question { get; set; }

    public virtual ScholarshipSubmission Submission { get; set; }
}
