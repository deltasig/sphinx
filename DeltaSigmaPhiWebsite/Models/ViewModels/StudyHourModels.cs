namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using System;
    using Entities;
    using System.Collections.Generic;

    public class TrackerModel
    {
        public int Offset { get; set; }
        public ProgressModel ThisWeek { get; set; }
        public Semester ThisSemester { get; set; }
    }
    public class ProgressModel
    {
        public IEnumerable<Member> Members { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}