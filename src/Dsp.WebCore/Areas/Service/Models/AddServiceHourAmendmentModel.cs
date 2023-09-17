namespace Dsp.WebCore.Areas.Service.Models
{
    using Dsp.Data.Entities;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class AddServiceHourAmendmentModel
    {
        public ServiceHourAmendment Amendment { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}