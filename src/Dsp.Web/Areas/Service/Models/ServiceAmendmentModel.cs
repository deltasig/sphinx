namespace Dsp.Web.Areas.Service.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceAmendmentModel
    {
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
        public List<ServiceHourAmendment> ServiceHourAmendments { get; set; }
        public List<ServiceEventAmendment> ServiceEventAmendments { get; set; }
    }
}