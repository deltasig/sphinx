namespace Dsp.Web.Areas.Admin.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class AppointmentModel
    {
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
        public IEnumerable<Position> Positions { get; set; }
    }
}