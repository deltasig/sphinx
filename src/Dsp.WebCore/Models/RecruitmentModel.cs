namespace Dsp.WebCore.Models;

using Dsp.Data.Entities;

public class RecruitmentModel
{
    public Semester Semester { get; set; }
    public IEnumerable<ScholarshipApp> ScholarshipApps { get; set; }
}