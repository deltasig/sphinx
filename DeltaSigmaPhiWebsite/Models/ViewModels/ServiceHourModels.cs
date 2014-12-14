namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceHourIndexFilterModel
    {
        public List<ServiceHourIndexModel> ServiceHours { get; set; }
        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
    public class ServiceHourIndexModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public Semester Semester { get; set; }
    }

    public class EventIndexFilterModel
    {
        public List<Event> Events { get; set; }
        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}