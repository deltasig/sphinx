namespace DeltaSigmaPhiWebsite.Models
{
    using System.Collections.Generic;
    using Entities;

    public class ExternalScholarshipModel
    {
        public IEnumerable<ScholarshipApp> Applications { get; set; }
        public IEnumerable<ScholarshipType> Types { get; set; }
    }
}