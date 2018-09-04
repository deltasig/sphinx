using Dsp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsp.Services.Interfaces
{
    public interface IBugService
    {
        Task<IEnumerable<BugReport>> GetBugReportsAsync(int page = 0, int pageSize = 10, bool includeFixed = false, string searchTerm = "");
        Task<int> GetBugReportCountAsync(bool includeFixed = false);
        Task<BugReport> GetBugReportByIdAsync(int id);

        Task CreateBugImageAsync(BugImage bugImage);
        Task CreateBugReportAsync(BugReport bugReport);

        Task UpdateBugReportAsync(BugReport bugReport);

        Task DeleteBugImageByIdAsync(int id);
        Task DeleteBugReportByIdAsync(int id);
    }
}