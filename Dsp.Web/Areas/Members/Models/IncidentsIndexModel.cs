namespace Dsp.Web.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class IncidentsIndexModel
    {
        public IEnumerable<IncidentReport> Incidents { get; set; }
    }
}