namespace Dsp.WebCore.Areas.Scholarships.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class SubmitScholarshipAppModel
{
    public ScholarshipApp App { get; set; }
    public ScholarshipSubmission Submission { get; set; }
    public IList<ScholarshipAnswer> Answers { get; set; } 
}