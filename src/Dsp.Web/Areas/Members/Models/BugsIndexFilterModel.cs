namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Web.Models;

    public class BugsIndexFilterModel : Pager
    {
        public bool includeFixed { get; set; }
        public string search { get; set; }

        public BugsIndexFilterModel()
        {
            this.includeFixed = false;
            this.search = string.Empty;

            base.page = 1;
            base.pageSize = 10;
        }
        public BugsIndexFilterModel(BugsIndexFilterModel original)
        {
            this.includeFixed = original.includeFixed;
            this.search = original.search;

            base.page = original.page;
            base.pageSize = original.pageSize;
        }
    }
}