namespace Dsp.Areas.Members.Models
{
    public class IncidentsIndexFilterModel
    {
        public int page { get; set; }
        public bool unresolved { get; set; }
        public bool resolved { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public IncidentsIndexFilterModel()
        {
            
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