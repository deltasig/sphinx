namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class IncidentService : BaseService, IIncidentService
    {
        private readonly IRepository _repository;

        public IncidentService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public IncidentService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public IncidentService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IncidentReport>> GetIncidentReportsAsync(
            int page = 1,
            int pageSize = 10,
            bool includeResolved = true,
            bool includeUnresolved = false,
            string sort = "oldest",
            string searchTerm = "")
        {
            page--;
            var lowerSearchTerm = searchTerm.ToLower();
            var entities = await _repository
                .GetAsync<IncidentReport>(
                    filter: x =>
                        (x.OfficialReport.ToLower().Contains(lowerSearchTerm) ||
                         x.Description.ToLower().Contains(lowerSearchTerm) ||
                         x.InvestigationNotes.ToLower().Contains(lowerSearchTerm)) && // Search term filter
                        (string.IsNullOrEmpty(x.OfficialReport) == includeResolved ||
                         !string.IsNullOrEmpty(x.OfficialReport) == includeUnresolved), // Resolved vs. unresolved
                    orderBy: x => sort == "oldest"
                        ? x.OrderBy(b => b.DateTimeSubmitted)
                        : x.OrderByDescending(b => b.DateTimeSubmitted),
                    skip: page * pageSize,
                    take: pageSize
                );
            foreach (var b in entities)
            {
                b.DateTimeOfIncident = ConvertUtcToCst(b.DateTimeOfIncident);
                b.DateTimeSubmitted = ConvertUtcToCst(b.DateTimeSubmitted);
            }
            return entities;
        }

        public async Task<IncidentReport> GetIncidentReportByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync<IncidentReport>(id);
            if (entity == null) throw new ArgumentException("Could not find the incident report with the given ID.");

            entity.DateTimeOfIncident = ConvertUtcToCst(entity.DateTimeOfIncident);
            entity.DateTimeSubmitted = ConvertUtcToCst(entity.DateTimeSubmitted);

            return entity;
        }

        public async Task CreateIncidentReportAsync(IncidentReport incidentReport)
        {
            _repository.Create(incidentReport);
            await _repository.SaveAsync();
        }

        public async Task UpdateIncidentReportAsync(IncidentReport incidentReport)
        {
            incidentReport.DateTimeOfIncident = ConvertCstToUtc(incidentReport.DateTimeOfIncident);
            incidentReport.DateTimeSubmitted = ConvertCstToUtc(incidentReport.DateTimeSubmitted);

            _repository.Update(incidentReport);
            await _repository.SaveAsync();
        }
    }
}
