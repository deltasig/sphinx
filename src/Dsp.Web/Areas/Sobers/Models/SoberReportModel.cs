namespace Dsp.Web.Areas.Sobers.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class SoberReportModel
    {
        public List<Member> Members { get; set; }
        public int? SelectedSemester { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }
    }
}