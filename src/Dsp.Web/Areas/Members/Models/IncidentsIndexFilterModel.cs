namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Web.Models;

    public class IncidentsIndexFilterModel : Pager
    {
        public bool unresolved { get; set; }
        public bool resolved { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public IncidentsIndexFilterModel()
        {
            this.unresolved = true;
            this.resolved = false;
            this.sort = "newest";
            this.s = string.Empty;

            base.page = 1;
            base.pageSize = 10;
        }
        public IncidentsIndexFilterModel(IncidentsIndexFilterModel original)
        {
            this.unresolved = original.unresolved;
            this.resolved = original.resolved;
            this.sort = original.sort;
            this.s = original.s;

            base.page = original.page;
            base.pageSize = original.pageSize;
        }
    }
}