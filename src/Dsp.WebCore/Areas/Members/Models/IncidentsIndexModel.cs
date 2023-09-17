namespace Dsp.WebCore.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class IncidentsIndexModel
    {
        public IEnumerable<IncidentReport> Incidents { get; set; }
        public IncidentsIndexFilterModel Filter { get; set; }
        public int TotalPages { get; set; }
        public int UnresolvedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ResultCount { get; set; }

        public IncidentsIndexModel(
            IEnumerable<IncidentReport> incidents,
            IncidentsIndexFilterModel filter,
            int totalPages,
            int unresolvedCount,
            int resolvedCount)
        {
            Incidents = incidents;
            Filter = filter;
            TotalPages = totalPages;
            UnresolvedCount = unresolvedCount;
            ResolvedCount = resolvedCount;
            ResultCount = UnresolvedCount + ResolvedCount;
        }
    }
}