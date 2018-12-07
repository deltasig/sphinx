namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIncidentService
    {
        Task<IEnumerable<IncidentReport>> GetIncidentReportsAsync(
            int page = 1,
            int pageSize = 10,
            bool includeResolved = true,
            bool includeUnresolved = false,
            string sort = "newest",
            string searchTerm = "");

        Task<IncidentReport> GetIncidentReportByIdAsync(int id);

        Task CreateIncidentReportAsync(IncidentReport incidentReport);

        Task UpdateIncidentReportAsync(IncidentReport incidentReport);
    }
}