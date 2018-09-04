namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class BugsIndexModel
    {
        public IEnumerable<BugReport> BugReports { get; set; }
        public BugsIndexFilterModel Filter { get; set; }
    }
}