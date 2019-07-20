using Dsp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsp.Services.Interfaces
{
    public interface IBugService : IService
    {
        Task<Tuple<IEnumerable<BugReport>, int, int, int>> GetBugReportsAsync(
            int page = 1,
            int pageSize = 10,
            bool includeFixed = false,
            string searchTerm = "");
        Task<int> GetBugReportCountAsync(bool includeFixed = false);
        Task<BugReport> GetBugReportByIdAsync(int id);

        Task CreateBugImageAsync(BugImage bugImage);
        Task CreateBugReportAsync(BugReport bugReport);

        Task UpdateBugReportAsync(BugReport bugReport);

        Task DeleteBugImageByIdAsync(int id);
        Task DeleteBugReportByIdAsync(int id);
    }
}