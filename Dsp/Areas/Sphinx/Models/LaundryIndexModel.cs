namespace Dsp.Areas.Dsp.Models
{
    using System.Collections.Generic;
    using Entities;
    using System.Web.Mvc;

    public class LaundryIndexModel
    {
        public IEnumerable<IEnumerable<LaundrySignup>> Slots { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}