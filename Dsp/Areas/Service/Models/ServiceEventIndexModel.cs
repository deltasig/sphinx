namespace Dsp.Areas.Service.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceEventIndexModel
    {
        public List<Event> Events { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}