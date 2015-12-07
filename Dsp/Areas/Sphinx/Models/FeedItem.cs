namespace Dsp.Areas.Sphinx.Models
{
    using System;

    public class FeedItem
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public string TimeSince { get; set; }
        public DateTime OccurredOn { get; set; }
        public string DisplayText { get; set; }
        public string Link { get; set; }
        public string Symbol { get; set; }
    }
}
