namespace Dsp.Web.Areas.Service.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceEventIndexModel
    {
        public List<ServiceEvent> Events { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}