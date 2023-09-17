namespace Dsp.WebCore.Areas.House.Models
{
    using Dsp.WebCore.Models;

    public class WorkOrderIndexFilterModel : Pager
    {
        public bool open { get; set; }
        public bool closed { get; set; }
        public string sort { get; set; }
        public string s { get; set; }

        public WorkOrderIndexFilterModel()
        {
            this.open = true;
            this.closed = false;
            this.sort = "newest";
            this.s = string.Empty;

            base.page = 1;
            base.pageSize = 10;
        }

        public WorkOrderIndexFilterModel(WorkOrderIndexFilterModel original)
        {
            this.open = original.open;
            this.closed = original.closed;
            this.sort = original.sort;
            this.s = original.s;

            base.page = original.page;
            base.pageSize = original.pageSize;
        }
    }
}