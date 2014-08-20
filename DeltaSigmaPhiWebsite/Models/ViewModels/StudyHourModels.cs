namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using System;
    using Entities;
    using System.Collections.Generic;

    public class TrackerModel
    {
        public ProgressModel ThisWeek { get; set; }
        public ProgressModel ThisSemester { get; set; } 
    }
    public class ProgressModel
    {
        public IEnumerable<Member> Members { get; set; }
        public DateTime StartDate { get; set; }
    }
}