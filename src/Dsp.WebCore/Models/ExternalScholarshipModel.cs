using Dsp.Data.Entities;

namespace Dsp.WebCore.Models;

public class ExternalScholarshipModel
{
    public IEnumerable<ScholarshipApp> Applications { get; set; }
    public IEnumerable<ScholarshipType> Types { get; set; }
}