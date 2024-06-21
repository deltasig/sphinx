namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;

    public class IncidentReportDetailsModel
    {
        public IncidentReport Report { get; set; }
        public bool CanEditReport { get; set; }
        public bool CanViewOriginalReport { get; set; }
        public bool CanViewInvestigationNotes { get; set; }
    }
}