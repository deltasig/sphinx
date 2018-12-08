namespace Dsp.Web.Models
{
    public class PagerModel
    {
        public Pager Previous { get; set; }
        public Pager Incrementer { get; set; }
        public Pager Next { get; set; }

        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int ResultCount { get; set; }
        public int CurrentPage { get; set; }
    }
}