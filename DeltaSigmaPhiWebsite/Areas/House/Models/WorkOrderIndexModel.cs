namespace DeltaSigmaPhiWebsite.Areas.House.Models
{
    public class WorkOrderIndexModel
    {
        public int page { get; set; }
        public bool open { get; set; }
        public bool closed { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public WorkOrderIndexModel()
        {
            
        }
        public WorkOrderIndexModel(WorkOrderIndexModel original)
        {
            page = original.page;
            open = original.open;
            closed = original.closed;
            sort = original.sort;
            s = original.s;
        }
    }
}