namespace Dsp.Areas.Service.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ServiceAddAmendmentModel
    {
        public ServiceAmendment Amendment { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}