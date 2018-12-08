namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class BugsIndexModel
    {
        public IEnumerable<BugReport> BugReports { get; set; }
        public BugsIndexFilterModel Filter { get; set; }
        public int TotalPages { get; set; }
        public int OpenCount { get; set; }
        public int FixedCount { get; set; }
        public int ResultCount { get; set; }

        public BugsIndexModel(
            IEnumerable<BugReport> bugReports,
            BugsIndexFilterModel filter,
            int totalPages,
            int unfixedCount,
            int fixedCount)
        {
            BugReports = bugReports;
            Filter = filter;
            TotalPages = totalPages;
            OpenCount = unfixedCount;
            FixedCount = fixedCount;
            ResultCount = OpenCount + FixedCount;
        }
    }
}