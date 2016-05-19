namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class IncidentsIndexModel
    {
        public IEnumerable<IncidentReport> Incidents { get; set; }
    }
}