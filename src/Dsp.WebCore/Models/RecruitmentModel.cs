using Dsp.Data.Entities;
using System.Collections.Generic;

namespace Dsp.WebCore.Models
{
    public class RecruitmentModel
    {
        public Semester Semester { get; set; }
        public IEnumerable<ScholarshipApp> ScholarshipApps { get; set; }
    }
}