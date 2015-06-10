namespace Dsp.Areas.Service.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Entities;

    public class ServiceHourIndexFilterModel
    {
        public List<ServiceHourIndexModel> ServiceHours { get; set; }
        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }

        public Semester Semester { get; set; }

        public Semester PreviousSemester { get; set; }
    }
}