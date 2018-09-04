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

    public class BugService : BaseService, IBugService
    {
        private readonly IRepository _repository;

        public BugService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public BugService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public BugService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BugReport>> GetBugReportsAsync(int page = 1, int pageSize = 10, bool includeFixed = false, string searchTerm = "")
        {
            page--;
            var lowerSearchTerm = searchTerm.ToLower();
            var bugReports = await _repository
                .GetAsync<BugReport>(
                    filter: x =>
                        (!x.IsFixed || x.IsFixed == includeFixed) &&
                        (x.Description.ToLower().Contains(lowerSearchTerm) || x.UrlWithProblem.ToLower().Contains(lowerSearchTerm)),
                    orderBy: x => x.OrderByDescending(b => b.ReportedOn),
                    skip: page * pageSize,
                    take: pageSize
                );
            foreach (var b in bugReports)
            {
                b.ReportedOn = ConvertUtcToCst(b.ReportedOn);
                b.LastUpdatedOn = ConvertUtcToCst(b.LastUpdatedOn);
            }
            return bugReports;
        }

        public async Task<int> GetBugReportCountAsync(bool includeFixed = false)
        {
            var count = await _repository.GetCountAsync<BugReport>(filter: x => !x.IsFixed || x.IsFixed == includeFixed);
            return count;
        }

        public async Task<BugReport> GetBugReportByIdAsync(int id)
        {
            var bugReport = await _repository.GetByIdAsync<BugReport>(id);
            if (bugReport == null) throw new ArgumentException("Could not find the bug report with the given ID.");
            bugReport.ReportedOn = ConvertUtcToCst(bugReport.ReportedOn);
            bugReport.LastUpdatedOn = ConvertUtcToCst(bugReport.LastUpdatedOn);
            return bugReport;
        }

        public async Task CreateBugReportAsync(BugReport bugReport)
        {
            bugReport.ReportedOn = DateTime.UtcNow;
            bugReport.LastUpdatedOn = bugReport.ReportedOn;

            _repository.Create(bugReport);
            await _repository.SaveAsync();
        }

        public async Task CreateBugImageAsync(BugImage bugImage)
        {
            _repository.Create(bugImage);
            await _repository.SaveAsync();
        }

        public async Task UpdateBugReportAsync(BugReport bugReport)
        {
            bugReport.ReportedOn = ConvertCstToUtc(bugReport.ReportedOn);
            bugReport.LastUpdatedOn = DateTime.UtcNow;

            _repository.Update(bugReport);
            await _repository.SaveAsync();
        }

        public async Task DeleteBugReportByIdAsync(int id)
        {
            _repository.Delete<BugReport>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteBugImageByIdAsync(int id)
        {
            _repository.Delete<BugImage>(id);
            await _repository.SaveAsync();
        }
    }
}
