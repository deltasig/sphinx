namespace Dsp.Areas.Service.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceHourIndexModel
    {
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
        public List<ServiceHourIndexMemberRowModel> ServiceHours { get; set; }
    }
}