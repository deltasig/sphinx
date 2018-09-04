namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Web.Models;

    public class BugsIndexFilterModel : Pager
    {
        public bool includeFixed { get; set; }
        public string search { get; set; }
        public int pages { get; set; }
        public int count { get; set; }


        public BugsIndexFilterModel()
        {
            page = 1;
            includeFixed = false;
            search = string.Empty;
            pageSize = 10;
        }
        public BugsIndexFilterModel(BugsIndexFilterModel original)
        {
            page = original.page;
            includeFixed = original.includeFixed;
            search = original.search;
            pageSize = original.pageSize;
        }
    }
}