namespace Dsp.Services;

using Data;
using Dsp.Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class IncidentService : BaseService, IIncidentService
{
    private readonly DspDbContext _context;

    public IncidentService(DspDbContext context)
    {
        _context = context;
    }

    public async Task<Tuple<IEnumerable<IncidentReport>, int, int, int>> GetIncidentReportsAsync(
        int page = 1,
        int pageSize = 10,
        bool includeResolved = true,
        bool includeUnresolved = false,
        string sort = "oldest",
        string searchTerm = "")
    {
        page--;
        if (page < 0) page = 0;
        var lowerSearchTerm = searchTerm?.ToLower() ?? string.Empty;

        var query = _context.IncidentReports
            .Where(x =>
                (x.PolicyBroken.ToLower().Contains(lowerSearchTerm) ||
                    x.OfficialReport.ToLower().Contains(lowerSearchTerm) ||
                    x.Description.ToLower().Contains(lowerSearchTerm) ||
                    x.InvestigationNotes.ToLower().Contains(lowerSearchTerm)) &&
                (string.IsNullOrEmpty(x.OfficialReport) && includeUnresolved ||
                    !string.IsNullOrEmpty(x.OfficialReport) && includeResolved)
            );
        if (sort == "oldest")
            query = query.OrderBy(b => b.DateTimeSubmitted);
        else
            query = query.OrderByDescending(b => b.DateTimeSubmitted);

        var filteredEntities = await query.Skip(pageSize * page).Take(pageSize).ToListAsync();
        foreach (var b in filteredEntities)
        {
            b.DateTimeOfIncident = ConvertUtcToCst(b.DateTimeOfIncident);
            b.DateTimeSubmitted = ConvertUtcToCst(b.DateTimeSubmitted);
        }

        var totalResults = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);
        var totalUnresolved = 0;
        var totalResolved = 0;
        if (includeUnresolved) totalUnresolved = await query.CountAsync(x => string.IsNullOrEmpty(x.OfficialReport));
        if (includeResolved) totalResolved = await query.CountAsync(x => !string.IsNullOrEmpty(x.OfficialReport));

        var result = new Tuple<IEnumerable<IncidentReport>, int, int, int>(filteredEntities, totalPages, totalUnresolved, totalResolved);
        return result;
    }

    public async Task<IncidentReport> GetIncidentReportByIdAsync(int id)
    {
        var entity = await _context.FindAsync<IncidentReport>(id);
        if (entity == null) throw new ArgumentException("Could not find the incident report with the given ID.");

        entity.DateTimeOfIncident = ConvertUtcToCst(entity.DateTimeOfIncident);
        entity.DateTimeSubmitted = ConvertUtcToCst(entity.DateTimeSubmitted);

        return entity;
    }

    public async Task CreateIncidentReportAsync(IncidentReport incidentReport)
    {
        _context.Add(incidentReport);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateIncidentReportAsync(IncidentReport incidentReport)
    {
        var existingReport = await GetIncidentReportByIdAsync(incidentReport.Id);

        existingReport.PolicyBroken = incidentReport.PolicyBroken;
        existingReport.InvestigationNotes = incidentReport.InvestigationNotes;
        existingReport.OfficialReport = incidentReport.OfficialReport;

        await _context.SaveChangesAsync();
    }
}
