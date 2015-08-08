namespace Dsp.Areas.House.Models
{
    using Dsp.Models;

    public class WorkOrderIndexFilterModel : Pager
    {
        public bool open { get; set; }
        public bool closed { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public WorkOrderIndexFilterModel()
        {
            
        }
        public WorkOrderIndexFilterModel(WorkOrderIndexFilterModel original)
        {
            page = original.page;
            open = original.open;
            closed = original.closed;
            sort = original.sort;
            s = original.s;
        }
    }
}