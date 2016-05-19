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
            page = 1;
            unresolved = true;
            resolved = false;
            sort = "newest";
            s = string.Empty;
        }
        public IncidentsIndexFilterModel(IncidentsIndexFilterModel original)
        {
            page = original.page;
            unresolved = original.unresolved;
            resolved = original.resolved;
            sort = original.sort;
            s = original.s;
        }
    }
}