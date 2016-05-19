namespace Dsp.Web.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class ExternalScholarshipModel
    {
        public IEnumerable<ScholarshipApp> Applications { get; set; }
        public IEnumerable<ScholarshipType> Types { get; set; }
    }
}