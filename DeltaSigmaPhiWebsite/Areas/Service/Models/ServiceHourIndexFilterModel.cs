namespace DeltaSigmaPhiWebsite.Areas.Service.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceHourIndexFilterModel
    {
        public List<ServiceHourIndexModel> ServiceHours { get; set; }
        public int? SelectedSemester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}