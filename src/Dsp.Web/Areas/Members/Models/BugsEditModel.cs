namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    public class BugsEditModel
    {
        public BugReport BugReport { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}