namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System.Collections.Generic;

    public class TrackerModel
    {
        public IEnumerable<StudyHour> ThisWeek { get; set; }
        public IEnumerable<StudyHour> ThisSemester { get; set; } 

    }
}