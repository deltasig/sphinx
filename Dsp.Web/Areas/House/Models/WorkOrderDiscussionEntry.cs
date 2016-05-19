namespace Dsp.Web.Areas.House.Models
{
    using System;

    public class WorkOrderDiscussionEntry
    {
        public DateTime OccurredOn { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string AvatarPath { get; set; }
        public string UserName { get; set; }
    }
}